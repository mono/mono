//---------------------------------------------------------------------
// <copyright file="ViewSimplifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft, Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Data.Common.Utils;
using System.Linq;
using System.Globalization;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.CommandTrees.Internal;

namespace System.Data.Common.CommandTrees.Internal
{

    /// <summary>
    /// Utility class that walks a mapping view and returns a simplified expression with projection
    /// nodes collapsed. Specifically recognizes the following common pattern in mapping views:
    /// 
    ///     outerProject(outerBinding(innerProject(innerBinding, innerNew)), outerProjection)
    ///     
    /// Recognizes simple disciminator patterns of the form:
    /// 
    ///     select 
    ///         case when Disc = value1 then value Type1(...)
    ///         case when Disc = value2 then value Type2(...)
    ///         ...
    ///         
    /// Recognizes redundant case statement of the form:
    /// 
    ///     select
    ///         case when (case when Predicate1 then true else false) ...
    /// 
    /// </summary>
    internal class ViewSimplifier
    {
        internal static DbQueryCommandTree SimplifyView(EntitySetBase extent, DbQueryCommandTree view)
        {
            ViewSimplifier vs = new ViewSimplifier(view.MetadataWorkspace, extent);
            view = vs.Simplify(view);
            return view;
        }

        private readonly MetadataWorkspace metadata;
        private readonly EntitySetBase extent;

        private ViewSimplifier(MetadataWorkspace mws, EntitySetBase viewTarget)
        {
            this.metadata = mws;
            this.extent = viewTarget;
        }

        private DbQueryCommandTree Simplify(DbQueryCommandTree view)
        {
            var simplifier = PatternMatchRuleProcessor.Create(
                // determines if an expression is of the form outerProject(outerProjection(innerProject(innerNew)))
                PatternMatchRule.Create(Pattern_CollapseNestedProjection, ViewSimplifier.CollapseNestedProjection),
                
                // A case statement can potentially be simplified
                PatternMatchRule.Create(Pattern_Case, ViewSimplifier.SimplifyCaseStatement),

                // Nested TPH discriminator pattern can be converted to the expected TPH discriminator pattern
                PatternMatchRule.Create(Pattern_NestedTphDiscriminator, ViewSimplifier.SimplifyNestedTphDiscriminator),

                // Entity constructors may be augmented with FK-based related entity refs
                PatternMatchRule.Create(Pattern_EntityConstructor, this.AddFkRelatedEntityRefs)
            );
                
            DbExpression queryExpression = view.Query;
            queryExpression = simplifier(queryExpression);

            view = DbQueryCommandTree.FromValidExpression(view.MetadataWorkspace, view.DataSpace, queryExpression);
            return view;
        }

        #region Navigation simplification support by adding FK-based related entity refs

        private static readonly Func<DbExpression, bool> Pattern_EntityConstructor =
            Patterns.MatchProject(
                Patterns.AnyExpression,
                Patterns.And(
                    Patterns.MatchEntityType,
                    Patterns.Or
                    (
                        Patterns.MatchNewInstance(),
                        Patterns.MatchCase(Patterns.AnyExpressions, Patterns.MatchForAll(Patterns.MatchNewInstance()), Patterns.MatchNewInstance())
                    )
                )
            );

        private bool doNotProcess;

        private DbExpression AddFkRelatedEntityRefs(DbExpression viewConstructor)
        {
            // If the extent being simplified is not a C-Space entity set, or if it has already
            // been processed by the simplifier, then keep the original expression by returning
            // null.
            //
            if (this.doNotProcess)
            {
                return null;
            }

            if(this.extent.BuiltInTypeKind != BuiltInTypeKind.EntitySet ||
               this.extent.EntityContainer.DataSpace != DataSpace.CSpace)
            {
                this.doNotProcess = true;
                return null;
            }

            // Get a reference to the entity set being simplified, and find all the foreign key
            // (foreign key) associations for which the association set references that entity set,
            // with either association end. 
            //
            EntitySet targetSet = (EntitySet)this.extent;
            var relSets = 
                targetSet.EntityContainer.BaseEntitySets
                .Where(es => es.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                .Cast<AssociationSet>()
                .Where(assocSet => 
                          assocSet.ElementType.IsForeignKey &&
                          assocSet.AssociationSetEnds.Any(se => se.EntitySet == targetSet)
                       )
                .ToList();
            
            // If no foreign key association sets that reference the entity set are present, then
            // no further processing is necessary, because FK-based related entity references cannot
            // be computed and added to the entities constructed for the entity set.
            if (relSets.Count == 0)
            {
                this.doNotProcess = true;
                return null;
            }

            // For every relationship set that references this entity set, the relationship type and
            // foreign key constraint are used to determine if the entity set is the dependent set.
            // If it is the dependent set, then it is possible to augment the view definition with a
            // related entity ref that represents the navigation of the relationship set's relationship
            // from the dependent end (this entity set) to the the principal end (the entity set that
            // is referenced by the other association set end of the relationship set).
            //
            var principalSetsAndDependentTypes = new HashSet<Tuple<EntityType, AssociationSetEnd, ReferentialConstraint>>();
            foreach (AssociationSet relSet in relSets)
            {
                // Retrieve the single referential constraint from the foreign key association, and
                // use it to determine whether the association set end that represents the dependent
                // end of the association references this entity set.
                //
                var fkConstraint = relSet.ElementType.ReferentialConstraints[0];
                var dependentSetEnd = relSet.AssociationSetEnds[fkConstraint.ToRole.Name];
                
                if (dependentSetEnd.EntitySet == targetSet)
                {
                    EntityType requiredSourceNavType = (EntityType)TypeHelpers.GetEdmType<RefType>(dependentSetEnd.CorrespondingAssociationEndMember.TypeUsage).ElementType;
                    var principalSetEnd = relSet.AssociationSetEnds[fkConstraint.FromRole.Name];

                    // Record the entity type that an element of this dependent entity set must have in order
                    // to be a valid navigation source for the relationship set's relationship, along with the
                    // association set end for the destination (principal) end of the navigation and the FK
                    // constraint that is associated with the relationship type. This information may be used
                    // later to construct a related entity ref for any entity constructor expression in the view
                    // that produces an entity of the required source type or a subtype.
                    //
                    principalSetsAndDependentTypes.Add(Tuple.Create(requiredSourceNavType, principalSetEnd, fkConstraint));
                }
            }

            // If no foreign key association sets that use the entity set as the dependent set are present,
            // then no further processing is possible, since FK-based related entity refs can only be added
            // to the view definition for navigations from the dependent end of the relationship to the principal.
            //
            if (principalSetsAndDependentTypes.Count == 0)
            {
                this.doNotProcess = true;
                return null;
            }

            // This rule supports a view that is capped with a projection of the form
            // (input).Project(x => new Entity()) 
            // or
            // (input).Project(x => CASE WHEN (condition1) THEN new Entity1() ELSE WHEN (condition2) THEN new Entity2()... ELSE new EntityN())
            // where every new instance expression Entity1()...EntityN() constructs an entity of a type
            // that is compatible with the entity set's element type.
            // Here, the list of all DbNewInstanceExpressions contained in the projection is remembered,
            // along with any CASE statement conditions, if present. These expressions will be updated
            // if necessary and used to build a new capping projection if any of the entity constructors
            // are augmented with FK-based related entity references.
            //
            DbProjectExpression entityProject = (DbProjectExpression)viewConstructor;
            List<DbNewInstanceExpression> constructors = new List<DbNewInstanceExpression>();
            List<DbExpression> conditions = null;
            if (entityProject.Projection.ExpressionKind == DbExpressionKind.Case)
            {
                // If the projection is a DbCaseExpression, then every result must be a DbNewInstanceExpression
                DbCaseExpression discriminatedConstructor = (DbCaseExpression)entityProject.Projection;
                conditions = new List<DbExpression>(discriminatedConstructor.When.Count);
                for (int idx = 0; idx < discriminatedConstructor.When.Count; idx++)
                {
                    conditions.Add(discriminatedConstructor.When[idx]);
                    constructors.Add((DbNewInstanceExpression)discriminatedConstructor.Then[idx]);
                }
                constructors.Add((DbNewInstanceExpression)discriminatedConstructor.Else);
            }
            else
            {
                // Otherwise, the projection must be a single DbNewInstanceExpression
                constructors.Add((DbNewInstanceExpression)entityProject.Projection);
            }
                        
            bool rebuildView = false;
            for (int idx = 0; idx < constructors.Count; idx++)
            {
                DbNewInstanceExpression entityConstructor = constructors[idx];
                EntityType constructedEntityType = TypeHelpers.GetEdmType<EntityType>(entityConstructor.ResultType);

                List<DbRelatedEntityRef> relatedRefs = 
                    principalSetsAndDependentTypes
                    .Where(psdt => constructedEntityType == psdt.Item1 || constructedEntityType.IsSubtypeOf(psdt.Item1))
                    .Select(psdt => RelatedEntityRefFromAssociationSetEnd(constructedEntityType, entityConstructor, psdt.Item2, psdt.Item3)).ToList();

                if (relatedRefs.Count > 0)
                {
                    if (entityConstructor.HasRelatedEntityReferences)
                    {
                        relatedRefs = entityConstructor.RelatedEntityReferences.Concat(relatedRefs).ToList();
                    }

                    entityConstructor = DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression(constructedEntityType, entityConstructor.Arguments, relatedRefs);
                    constructors[idx] = entityConstructor;
                    rebuildView = true;
                }
            }

            // Default to returning null to indicate that this rule did not produce a modified expression
            //
            DbExpression result = null;
            if (rebuildView)
            {
                // rebuildView is true, so entity constructing DbNewInstanceExpression(s) were encountered
                // and updated with additional related entity refs. The DbProjectExpression that caps the
                // view definition therefore needs to be rebuilt and returned as the result of this rule.
                //
                if (conditions != null)
                {
                    // The original view definition projection was a DbCaseExpression.
                    // The new expression is also a DbCaseExpression that uses the conditions from the 
                    // original expression together with the updated result expressions to produce the
                    // new capping projection.
                    //
                    List<DbExpression> whens = new List<DbExpression>(conditions.Count);
                    List<DbExpression> thens = new List<DbExpression>(conditions.Count);
                    for (int idx = 0; idx < conditions.Count; idx++)
                    {
                        whens.Add(conditions[idx]);
                        thens.Add(constructors[idx]);
                    }

                    result = entityProject.Input.Project(DbExpressionBuilder.Case(whens, thens, constructors[conditions.Count]));
                }
                else
                {
                    // Otherwise, the capping projection consists entirely of the updated DbNewInstanceExpression.
                    //
                    result = entityProject.Input.Project(constructors[0]);
                }
            }

            // Regardless of whether or not the view was updated, this rule should not be applied again during rule processing
            this.doNotProcess = true;
            return result;
        }

        private static DbRelatedEntityRef RelatedEntityRefFromAssociationSetEnd(EntityType constructedEntityType, DbNewInstanceExpression entityConstructor, AssociationSetEnd principalSetEnd, ReferentialConstraint fkConstraint)
        {
            EntityType principalEntityType = (EntityType)TypeHelpers.GetEdmType<RefType>(fkConstraint.FromRole.TypeUsage).ElementType;
            IList<DbExpression> principalKeyValues = null;
            
            // Create Entity Property/DbExpression value pairs from the entity constructor DbExpression,
            // then join these with the principal/dependent property pairs from the FK constraint
            // to produce principal property name/DbExpression value pairs from which to create the principal ref.
            //
            // Ideally the code would be as below, but anonymous types break asmmeta:
            //var keyPropAndValue =
            //    from pv in constructedEntityType.Properties.Select((p, idx) => new { DependentProperty = p, Value = entityConstructor.Arguments[idx] })
            //    join ft in fkConstraint.FromProperties.Select((fp, idx) => new { PrincipalProperty = fp, DependentProperty = fkConstraint.ToProperties[idx] })
            //    on pv.DependentProperty equals ft.DependentProperty
            //    select new { PrincipalProperty = ft.PrincipalProperty.Name, Value = pv.Value };
            //
            var keyPropAndValue =
                from pv in constructedEntityType.Properties.Select((p, idx) => Tuple.Create(p, entityConstructor.Arguments[idx])) // new { DependentProperty = p, Value = entityConstructor.Arguments[idx] })
                join ft in fkConstraint.FromProperties.Select((fp, idx) => Tuple.Create(fp, fkConstraint.ToProperties[idx])) //new { PrincipalProperty = fp, DependentProperty = fkConstraint.ToProperties[idx] })
                on pv.Item1 equals ft.Item2 //pv.DependentProperty equals ft.DependentProperty
                select Tuple.Create(ft.Item1.Name, pv.Item2); // new { PrincipalProperty = ft.PrincipalProperty.Name, Value = pv.Value };

            // If there is only a single property in the principal's key, then there is no ordering concern.
            // Otherwise, create a dictionary of principal key property name to DbExpression value so that
            // when used as the arguments to the ref expression, the dependent property values - used here
            // as principal key property values - are in the correct order, which is the same order as the
            // key members themselves.
            //
            if (fkConstraint.FromProperties.Count == 1)
            {
                var singleKeyNameAndValue = keyPropAndValue.Single();
                Debug.Assert(singleKeyNameAndValue.Item1 == fkConstraint.FromProperties[0].Name, "Unexpected single key property name");
                principalKeyValues = new[] { singleKeyNameAndValue.Item2 };
            }
            else
            {
                var keyValueMap = keyPropAndValue.ToDictionary(pav => pav.Item1, pav => pav.Item2, StringComparer.Ordinal);
                principalKeyValues = principalEntityType.KeyMemberNames.Select(memberName => keyValueMap[memberName]).ToList();
            }
                        
            // Create the ref to the principal entity based on the (now correctly ordered) key value expressions.
            //
            DbRefExpression principalRef = principalSetEnd.EntitySet.CreateRef(principalEntityType, principalKeyValues);
            DbRelatedEntityRef result = DbExpressionBuilder.CreateRelatedEntityRef(fkConstraint.ToRole, fkConstraint.FromRole, principalRef);

            return result;
        }

        #endregion

        #region Nested TPH Discriminator simplification

        /// <summary>
        /// Matches the nested TPH discriminator pattern produced by view generation 
        /// </summary>
        private static readonly Func<DbExpression, bool> Pattern_NestedTphDiscriminator =
            Patterns.MatchProject(
                Patterns.MatchFilter(
                    Patterns.MatchProject(
                        Patterns.MatchFilter(
                            Patterns.AnyExpression,
                            Patterns.Or(
                                Patterns.MatchKind(DbExpressionKind.Equals),
                                Patterns.MatchKind(DbExpressionKind.Or)
                            )
                        ),
                        Patterns.And(
                            Patterns.MatchRowType,
                            Patterns.MatchNewInstance(
                                Patterns.MatchForAll(
                                    Patterns.Or(
                                        Patterns.And(
                                            Patterns.MatchNewInstance(),
                                            Patterns.MatchComplexType
                                        ),
                                        Patterns.MatchKind(DbExpressionKind.Property),
                                        Patterns.MatchKind(DbExpressionKind.Case)
                                    )
                                 )
                            )
                        )
                    ),
                    Patterns.Or(
                        Patterns.MatchKind(DbExpressionKind.Property),
                        Patterns.MatchKind(DbExpressionKind.Or)
                    )
                ),
                Patterns.And(
                    Patterns.MatchEntityType,
                    Patterns.MatchCase(
                        Patterns.MatchForAll(Patterns.MatchKind(DbExpressionKind.Property)),
                        Patterns.MatchForAll(Patterns.MatchKind(DbExpressionKind.NewInstance)),
                        Patterns.MatchKind(DbExpressionKind.NewInstance)
                    )
                )
            );
                
        /// <summary>
        /// Converts the DbExpression equivalent of:
        /// 
        /// SELECT CASE
        ///     WHEN a._from0 THEN SUBTYPE1()
        ///     ...
        ///     WHEN a._from[n-2] THEN SUBTYPE_n-1()
        ///     ELSE SUBTYPE_n
        /// FROM
        ///     SELECT
        ///         b.C1..., b.Cn
        ///         CASE WHEN b.Discriminator = SUBTYPE1_Value THEN true ELSE false AS _from0
        ///         ...
        ///         CASE WHEN b.Discriminator = SUBTYPE_n_Value THEN true ELSE false AS _from[n-1]
        ///     FROM TSet AS b
        ///     WHERE b.Discriminator = SUBTYPE1_Value... OR x.Discriminator = SUBTYPE_n_Value
        /// AS a
        /// WHERE a._from0... OR a._from[n-1]
        /// 
        /// into the DbExpression equivalent of the following, which is matched as a TPH discriminator
        /// by the <see cref="System.Data.Mapping.ViewGeneration.GeneratedView"/> class and so allows a <see cref="System.Data.Mapping.ViewGeneration.DiscriminatorMap"/>
        /// to be produced for the view, which would not otherwise be possible. Note that C1 through Cn
        /// are only allowed to be scalars or complex type constructors based on direct property references
        /// to the store entity set's scalar properties.
        /// 
        /// SELECT CASE
        ///     WHEN y.Discriminator = SUBTTYPE1_Value THEN SUBTYPE1()
        ///     ...
        ///     WHEN y.Discriminator = SUBTYPE_n-1_Value THEN SUBTYPE_n-1()
        ///     ELSE SUBTYPE_n()
        /// FROM
        ///     SELECT x.C1..., x.Cn, Discriminator FROM TSet AS x
        ///     WHERE x.Discriminator = SUBTYPE1_Value... OR x.Discriminator = SUBTYPE_n_Value 
        /// AS y
        ///     
        /// </summary>
        private static DbExpression SimplifyNestedTphDiscriminator(DbExpression expression)
        {
            DbProjectExpression entityProjection = (DbProjectExpression)expression;
            DbFilterExpression booleanColumnFilter = (DbFilterExpression)entityProjection.Input.Expression;
            DbProjectExpression rowProjection = (DbProjectExpression)booleanColumnFilter.Input.Expression;
            DbFilterExpression discriminatorFilter = (DbFilterExpression)rowProjection.Input.Expression;
            
            List<DbExpression> predicates = FlattenOr(booleanColumnFilter.Predicate).ToList();
            List<DbPropertyExpression> propertyPredicates =
                predicates.OfType<DbPropertyExpression>()
                .Where(px => px.Instance.ExpressionKind == DbExpressionKind.VariableReference &&
                             ((DbVariableReferenceExpression)px.Instance).VariableName == booleanColumnFilter.Input.VariableName).ToList();
            if (predicates.Count != propertyPredicates.Count)    
            {
                return null;
            }

            List<string> predicateColumnNames = propertyPredicates.Select(px => px.Property.Name).ToList();
                           
            Dictionary<object, DbComparisonExpression> discriminatorPredicates = new Dictionary<object, DbComparisonExpression>();
            if (!TypeSemantics.IsEntityType(discriminatorFilter.Input.VariableType) ||
                !TryMatchDiscriminatorPredicate(discriminatorFilter, (compEx, discValue) => discriminatorPredicates.Add(discValue, compEx)))
            {
                return null;
            }

            EdmProperty discriminatorProp = (EdmProperty)((DbPropertyExpression)((DbComparisonExpression)discriminatorPredicates.First().Value).Left).Property;
            DbNewInstanceExpression rowConstructor = (DbNewInstanceExpression)rowProjection.Projection;
            RowType resultRow = TypeHelpers.GetEdmType<RowType>(rowConstructor.ResultType);
            Dictionary<string, DbComparisonExpression> inputPredicateMap = new Dictionary<string, DbComparisonExpression>();
            Dictionary<string, DbComparisonExpression> selectorPredicateMap = new Dictionary<string, DbComparisonExpression>();
            Dictionary<string, DbExpression> columnValues = new Dictionary<string, DbExpression>(rowConstructor.Arguments.Count);
            for (int idx = 0; idx < rowConstructor.Arguments.Count; idx++)
            {
                string propName = resultRow.Properties[idx].Name;
                DbExpression columnVal = rowConstructor.Arguments[idx];
                if (predicateColumnNames.Contains(propName))
                {
                    if(columnVal.ExpressionKind != DbExpressionKind.Case)
                    {
                        return null;
                    }
                    DbCaseExpression casePredicate = (DbCaseExpression)columnVal;
                    if(casePredicate.When.Count != 1 ||
                       !TypeSemantics.IsBooleanType(casePredicate.Then[0].ResultType) || !TypeSemantics.IsBooleanType(casePredicate.Else.ResultType) ||
                        casePredicate.Then[0].ExpressionKind != DbExpressionKind.Constant || casePredicate.Else.ExpressionKind != DbExpressionKind.Constant ||
                        (bool)((DbConstantExpression)casePredicate.Then[0]).Value != true || (bool)((DbConstantExpression)casePredicate.Else).Value != false)
                    {
                        return null;
                    }

                    DbPropertyExpression comparedProp;
                    object constValue;
                    if(!TryMatchPropertyEqualsValue(casePredicate.When[0], rowProjection.Input.VariableName, out comparedProp, out constValue) ||
                       comparedProp.Property != discriminatorProp ||
                       !discriminatorPredicates.ContainsKey(constValue))
                    {
                        return null;
                    }

                    inputPredicateMap.Add(propName, discriminatorPredicates[constValue]);
                    selectorPredicateMap.Add(propName, (DbComparisonExpression)casePredicate.When[0]);
                }
                else
                {
                    columnValues.Add(propName, columnVal);
                }
            }

            // Build a new discriminator-based filter that only includes the same rows allowed by the higher '_from0' column-based filter
            DbExpression newDiscriminatorPredicate = Helpers.BuildBalancedTreeInPlace<DbExpression>(new List<DbExpression>(inputPredicateMap.Values), (left, right) => DbExpressionBuilder.Or(left, right));
            discriminatorFilter = discriminatorFilter.Input.Filter(newDiscriminatorPredicate);

            DbCaseExpression entitySelector = (DbCaseExpression)entityProjection.Projection;
            List<DbExpression> newWhens = new List<DbExpression>(entitySelector.When.Count);
            List<DbExpression> newThens = new List<DbExpression>(entitySelector.Then.Count);
            
            for (int idx = 0; idx < entitySelector.When.Count; idx++)
            {
                DbPropertyExpression propWhen = (DbPropertyExpression)entitySelector.When[idx];
                DbNewInstanceExpression entityThen = (DbNewInstanceExpression)entitySelector.Then[idx];

                DbComparisonExpression discriminatorWhen;
                if (!selectorPredicateMap.TryGetValue(propWhen.Property.Name, out discriminatorWhen))
                {
                    return null;
                }
                newWhens.Add(discriminatorWhen);

                DbExpression inputBoundEntityConstructor = ValueSubstituter.Substitute(entityThen, entityProjection.Input.VariableName, columnValues);
                newThens.Add(inputBoundEntityConstructor);
            }

            DbExpression newElse = ValueSubstituter.Substitute(entitySelector.Else, entityProjection.Input.VariableName, columnValues);
            DbCaseExpression newEntitySelector = DbExpressionBuilder.Case(newWhens, newThens, newElse);

            DbExpression result = discriminatorFilter.BindAs(rowProjection.Input.VariableName).Project(newEntitySelector);
            return result;
        }

        private class ValueSubstituter : DefaultExpressionVisitor
        {
            internal static DbExpression Substitute(DbExpression original, string referencedVariable, Dictionary<string, DbExpression> propertyValues)
            {
                Debug.Assert(original != null, "Original expression cannot be null");
                ValueSubstituter visitor = new ValueSubstituter(referencedVariable, propertyValues);
                return visitor.VisitExpression(original);
            }

            private readonly string variableName;
            private readonly Dictionary<string, DbExpression> replacements;

            private ValueSubstituter(string varName, Dictionary<string, DbExpression> replValues)
            {
                Debug.Assert(varName != null, "Variable name cannot be null");
                Debug.Assert(replValues != null, "Replacement values cannot be null");

                this.variableName = varName;
                this.replacements = replValues;
            }

            public override DbExpression Visit(DbPropertyExpression expression)
            {
                DbExpression result = null;

                DbExpression replacementValue;
                if (expression.Instance.ExpressionKind == DbExpressionKind.VariableReference &&
                    (((DbVariableReferenceExpression)expression.Instance).VariableName == this.variableName) &&
                    this.replacements.TryGetValue(expression.Property.Name, out replacementValue))
                {
                    result = replacementValue;
                }
                else
                {
                    result = base.Visit(expression);
                }
                return result;
            }
        }

        #endregion

        #region Case Statement Simplification

        /// <summary>
        /// Matches any Case expression 
        /// </summary>
        private static readonly Func<DbExpression, bool> Pattern_Case = Patterns.MatchKind(DbExpressionKind.Case);

        private static DbExpression SimplifyCaseStatement(DbExpression expression)
        {
            DbCaseExpression caseExpression = (DbCaseExpression)expression;

            // try simplifying predicates
            bool predicateSimplified = false;
            List<DbExpression> rewrittenPredicates = new List<DbExpression>(caseExpression.When.Count);
            foreach (var when in caseExpression.When)
            {
                DbExpression simplifiedPredicate;
                if (TrySimplifyPredicate(when, out simplifiedPredicate))
                {
                    rewrittenPredicates.Add(simplifiedPredicate);
                    predicateSimplified = true;
                }
                else
                {
                    rewrittenPredicates.Add(when);
                }
            }

            if (!predicateSimplified) { return null; }

            caseExpression = DbExpressionBuilder.Case(rewrittenPredicates, caseExpression.Then, caseExpression.Else);
            return caseExpression;
        }

        private static bool TrySimplifyPredicate(DbExpression predicate, out DbExpression simplified)
        {
            simplified = null;
            if (predicate.ExpressionKind != DbExpressionKind.Case) { return false; }
            var caseExpression = (DbCaseExpression)predicate;
            if (caseExpression.Then.Count != 1 && caseExpression.Then[0].ExpressionKind == DbExpressionKind.Constant)
            {
                return false;
            }
            var then = (DbConstantExpression)caseExpression.Then[0];
            if (!true.Equals(then.Value)) { return false; }
            if (caseExpression.Else != null)
            {
                if (caseExpression.Else.ExpressionKind != DbExpressionKind.Constant) { return false; }
                var when = (DbConstantExpression)caseExpression.Else;
                if (!false.Equals(when.Value)) { return false; }
            }
            simplified = caseExpression.When[0];
            return true;
        }

        #endregion

        #region Nested Projection Collapsing
                
        /// <summary>
        /// Determines if an expression is of the form outerProject(outerProjection(innerProject(innerNew)))
        /// </summary>
        private static readonly Func<DbExpression, bool> Pattern_CollapseNestedProjection =
            Patterns.MatchProject(
                Patterns.MatchProject(
                    Patterns.AnyExpression,
                    Patterns.MatchKind(DbExpressionKind.NewInstance)
                 ),
                 Patterns.AnyExpression
            );

        /// <summary>
        /// Collapses outerProject(outerProjection(innerProject(innerNew)))
        /// </summary>
        private static DbExpression CollapseNestedProjection(DbExpression expression)
        {
            DbProjectExpression outerProject = (DbProjectExpression)expression;
            DbExpression outerProjection = outerProject.Projection;
            DbProjectExpression innerProject = (DbProjectExpression)outerProject.Input.Expression;
            DbNewInstanceExpression innerNew = (DbNewInstanceExpression)innerProject.Projection;

            // get membername -> expression bindings for the inner select so that we know how map property
            // references to the inner projection
            Dictionary<string, DbExpression> bindings = new Dictionary<string, DbExpression>(innerNew.Arguments.Count);
            TypeUsage innerResultTypeUsage = innerNew.ResultType;
            RowType innerResultType = (RowType)innerResultTypeUsage.EdmType;

            for (int ordinal = 0; ordinal < innerResultType.Members.Count; ordinal++)
            {
                bindings[innerResultType.Members[ordinal].Name] = innerNew.Arguments[ordinal];
            }

            // initialize an expression visitor that knows how to map arguments to the outer projection
            // to the inner projection source
            ProjectionCollapser collapser = new ProjectionCollapser(bindings, outerProject.Input);

            // replace all property references to the inner projection
            var replacementOuterProjection = collapser.CollapseProjection(outerProjection);
            
            // make sure the collapsing was successful; if not, give up on simplification
            if (collapser.IsDoomed) { return null; }
            
            // set replacement value so that the expression replacer infrastructure can substitute
            // the collapsed projection in the expression tree
            // continue collapsing projection until the pattern no longer matches
            DbProjectExpression replacementOuterProject = innerProject.Input.Project(replacementOuterProjection);
            return replacementOuterProject;
        }

        /// <summary>
        /// This expression visitor supports collapsing a nested projection matching the pattern described above.
        /// 
        /// For instance:
        ///
        ///     select T.a as x, T.b as y, true as z from (select E.a as x, E.b as y from Extent E)
        ///
        /// resolves to:
        ///
        ///     select E.a, E.b, true as z from Extent E
        ///
        /// In general, 
        ///
        ///     outerProject(
        ///         outerBinding(
        ///             innerProject(innerBinding, innerNew)
        ///         ), 
        ///         outerNew)
        ///
        /// resolves to:
        ///
        ///     replacementOuterProject(
        ///         innerBinding, 
        ///         replacementOuterNew)
        ///
        /// The outer projection is bound to the inner input source (outerBinding -> innerBinding) and
        /// the outer new instance expression has its properties remapped to the inner new instance
        /// expression member expressions.
        /// 
        /// This replacer is used to simplify argument value in a new instance expression OuterNew
        /// from an expression of the form:
        ///
        ///      outerProject(outerBinding(innerProject(innerBinding, innerNew)), outerProjection)
        ///
        /// The replacer collapses the outer project terms to point at the innerNew expression.
        /// Where possible, VarRef_outer.Property_outer is collapsed to VarRef_inner.Property.
        /// </summary>
        private class ProjectionCollapser : DefaultExpressionVisitor
        {
            // the replacer context keeps track of member bindings for var refs and the expression
            // binding for the outer projection being remapped
            private Dictionary<string, DbExpression> m_varRefMemberBindings;
            private DbExpressionBinding m_outerBinding;
            private bool m_doomed;
            internal ProjectionCollapser(Dictionary<string, DbExpression> varRefMemberBindings,
                DbExpressionBinding outerBinding)
                : base()
            {
                m_varRefMemberBindings = varRefMemberBindings;
                m_outerBinding = outerBinding;
            }

            // Visit and identify the Property(VarRef "Outer binding") pattern,
            // remapping the property to the appropriate inner projection member
            internal DbExpression CollapseProjection(DbExpression expression)
            {
                return this.VisitExpression(expression);
            }

            public override DbExpression Visit(DbPropertyExpression property)
            {
                // check for a property of the outer projection binding (that can be remapped)
                if (property.Instance.ExpressionKind == DbExpressionKind.VariableReference &&
                        IsOuterBindingVarRef((DbVariableReferenceExpression)property.Instance))
                {
                    return m_varRefMemberBindings[property.Property.Name];
                }
                return base.Visit(property);
            }

            public override DbExpression Visit(DbVariableReferenceExpression varRef)
            {
                // if we encounter an unsubstitutued var ref, give up...
                if (IsOuterBindingVarRef(varRef))
                {
                    m_doomed = true;
                }
                return base.Visit(varRef);
            }

            /// <summary>
            /// Heuristic check to make sure the var ref is the one we're supposed to be replacing.
            /// </summary>
            private bool IsOuterBindingVarRef(DbVariableReferenceExpression varRef)
            {
                return varRef.VariableName == m_outerBinding.VariableName;
            }

            /// <summary>
            /// Returns a value indicating that the transformation has failed.
            /// </summary>
            internal bool IsDoomed
            {
                get { return m_doomed; }
            }
        }

        #endregion

        #region Utility Methods

        internal static IEnumerable<DbExpression> FlattenOr(DbExpression expression)
        {
            return Helpers.GetLeafNodes(expression,
                exp => (exp.ExpressionKind != DbExpressionKind.Or),
                exp => { DbOrExpression orExp = (DbOrExpression)exp; return new[] { orExp.Left, orExp.Right }; });
        }

        internal static bool TryMatchDiscriminatorPredicate(DbFilterExpression filter, Action<DbComparisonExpression, object> onMatchedComparison)
        {
            EdmProperty discriminatorProperty = null;

            // check each assignment in predicate
            foreach (var term in FlattenOr(filter.Predicate))
            {
                DbPropertyExpression currentDiscriminator;
                object discriminatorValue;
                if (!TryMatchPropertyEqualsValue(term, filter.Input.VariableName, out currentDiscriminator, out discriminatorValue))
                {
                    return false;
                }

                // must be the same discriminator in every case
                if (null == discriminatorProperty)
                {
                    discriminatorProperty = (EdmProperty)currentDiscriminator.Property;
                }
                else if (discriminatorProperty != currentDiscriminator.Property)
                {
                    return false;
                }

                onMatchedComparison((DbComparisonExpression)term, discriminatorValue);
            }

            return true;
        }

        internal static bool TryMatchPropertyEqualsValue(DbExpression expression, string propertyVariable, out DbPropertyExpression property, out object value)
        {
            property = null;
            value = null;
            // make sure when is of the form Discriminator = Constant
            if (expression.ExpressionKind != DbExpressionKind.Equals) { return false; }
            var equals = (DbBinaryExpression)expression;
            if (equals.Left.ExpressionKind != DbExpressionKind.Property) { return false; }
            property = (DbPropertyExpression)equals.Left;
            if (!TryMatchConstant(equals.Right, out value)) { return false; }

            // verify the property is a property of the input variable
            if (property.Instance.ExpressionKind != DbExpressionKind.VariableReference ||
                ((DbVariableReferenceExpression)property.Instance).VariableName != propertyVariable) { return false; }

            return true;
        }

        private static bool TryMatchConstant(DbExpression expression, out object value)
        {
            if (expression.ExpressionKind == DbExpressionKind.Constant)
            {
                value = ((DbConstantExpression)expression).Value;
                return true;
            }
            if (expression.ExpressionKind == DbExpressionKind.Cast &&
                expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                var castExpression = (DbCastExpression)expression;
                if (TryMatchConstant(castExpression.Argument, out value))
                {
                    // convert the value
                    var primitiveType = (PrimitiveType)expression.ResultType.EdmType;

                    // constant literals have already been validated by view gen...
                    value = Convert.ChangeType(value, primitiveType.ClrEquivalentType, CultureInfo.InvariantCulture);
                    return true;
                }
            }
            value = null;
            return false;
        }

        #endregion
    }
}
