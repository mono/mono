namespace System.Net {
    using System;
    using System.Configuration;
    using System.ComponentModel;
    
    // NOTE [Microsoft]: The old validation attribute was removed from System.ll and is     
    // replaced by more flexible and robust validation/conversion design.
    // The change bellow is a simple fix to make things work with the least possible change ( it is an integration break )
    // However, we already have a built-in support for configuration properties that store     
    // Type names. We do reccomend that all uses of the validator bellow are converted to
    // properties of type Type ( instead of string ) which use the TypeNameConverter from System.Configuration.dll
    // Feel free to ask me for more details if you decide to do the conversion
    internal sealed class TimeoutValidator : ConfigurationValidatorBase
    {
        bool _zeroValid = false;

        internal TimeoutValidator(bool zeroValid) {
            _zeroValid = zeroValid;
        }

        public override bool CanValidate( Type type ) {
            return ( type == typeof( int ) || type == typeof( long ) );
        }

        public override void Validate( object value ) {
            if (value == null)
                return;
            
            int timeout = (int)value;
            
            if (_zeroValid && timeout == 0)
                return;
            
            if (timeout <= 0 && timeout != System.Threading.Timeout.Infinite) {
                // Note [Microsoft] : This is a lab integration fix. Old code did not have any error message at this point
                // This code change accomplishes the same result. However its highly reccomended that a specific error message is givven
                // to the user so they know what exaclty is the problem ( i.e. the value must be a positive integer or be Infinite )
                // To accomplish this - an exception with the specific error message could be thrown ( ArgumentException is prefferred )            
                throw new ConfigurationErrorsException(SR.GetString(SR.net_io_timeout_use_gt_zero));
            }
        }
    }
}
