/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Extensions.ReplicationConstants.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Extensions
{
	
	/*
	* public class ReplicationConstants
	*/
	
	/// <summary> Contains a collection of constants used by the replication management 
	/// in Novell Ldap extensions.
	/// </summary>
	public class ReplicationConstants
	{
		
		/// <summary> A constant for the SplitPartitionRequest OID.</summary>
		public const System.String CREATE_NAMING_CONTEXT_REQ = "2.16.840.1.113719.1.27.100.3";
		
		/// <summary> A constant for the SplitPartitionResponse OID.</summary>
		public const System.String CREATE_NAMING_CONTEXT_RES = "2.16.840.1.113719.1.27.100.4";
		
		/// <summary> A constant for the mergePartitionRequest OID.</summary>
		public const System.String MERGE_NAMING_CONTEXT_REQ = "2.16.840.1.113719.1.27.100.5";
		
		/// <summary> A constant for the mergePartitionResponse OID.</summary>
		public const System.String MERGE_NAMING_CONTEXT_RES = "2.16.840.1.113719.1.27.100.6";
		
		/// <summary> A constant for the addReplicaRequest OID.</summary>
		public const System.String ADD_REPLICA_REQ = "2.16.840.1.113719.1.27.100.7";
		
		/// <summary> A constant for the addReplicaResponse OID.</summary>
		public const System.String ADD_REPLICA_RES = "2.16.840.1.113719.1.27.100.8";
		
		/// <summary> A constant for the refreshServerRequest OID.</summary>
		public const System.String REFRESH_SERVER_REQ = "2.16.840.1.113719.1.27.100.9";
		
		/// <summary> A constant for the refreshServerResponse OID.</summary>
		public const System.String REFRESH_SERVER_RES = "2.16.840.1.113719.1.27.100.10";
		
		/// <summary> A constant for the removeReplicaRequest OID.</summary>
		public const System.String DELETE_REPLICA_REQ = "2.16.840.1.113719.1.27.100.11";
		
		/// <summary> A constant for the removeReplicaResponse OID.</summary>
		public const System.String DELETE_REPLICA_RES = "2.16.840.1.113719.1.27.100.12";
		
		/// <summary> A constant for the partitionEntryCountRequest OID.</summary>
		public const System.String NAMING_CONTEXT_COUNT_REQ = "2.16.840.1.113719.1.27.100.13";
		
		/// <summary> A constant for the partitionEntryCountResponse OID.</summary>
		public const System.String NAMING_CONTEXT_COUNT_RES = "2.16.840.1.113719.1.27.100.14";
		
		/// <summary> A constant for the changeReplicaTypeRequest OID.</summary>
		public const System.String CHANGE_REPLICA_TYPE_REQ = "2.16.840.1.113719.1.27.100.15";
		
		/// <summary> A constant for the changeReplicaTypeResponse OID.</summary>
		public const System.String CHANGE_REPLICA_TYPE_RES = "2.16.840.1.113719.1.27.100.16";
		
		/// <summary> A constant for the getReplicaInfoRequest OID.</summary>
		public const System.String GET_REPLICA_INFO_REQ = "2.16.840.1.113719.1.27.100.17";
		
		/// <summary> A constant for the getReplicaInfoResponse OID.</summary>
		public const System.String GET_REPLICA_INFO_RES = "2.16.840.1.113719.1.27.100.18";
		
		/// <summary> A constant for the listReplicaRequest OID.</summary>
		public const System.String LIST_REPLICAS_REQ = "2.16.840.1.113719.1.27.100.19";
		
		/// <summary> A constant for the listReplicaResponse OID.</summary>
		public const System.String LIST_REPLICAS_RES = "2.16.840.1.113719.1.27.100.20";
		
		/// <summary> A constant for the receiveAllUpdatesRequest OID.</summary>
		public const System.String RECEIVE_ALL_UPDATES_REQ = "2.16.840.1.113719.1.27.100.21";
		
		/// <summary> A constant for the receiveAllUpdatesResponse OID.</summary>
		public const System.String RECEIVE_ALL_UPDATES_RES = "2.16.840.1.113719.1.27.100.22";
		
		/// <summary> A constant for the sendAllUpdatesRequest OID.</summary>
		public const System.String SEND_ALL_UPDATES_REQ = "2.16.840.1.113719.1.27.100.23";
		
		/// <summary> A constant for the sendAllUpdatesResponse OID.</summary>
		public const System.String SEND_ALL_UPDATES_RES = "2.16.840.1.113719.1.27.100.24";
		
		/// <summary> A constant for the requestPartitionSyncRequest OID.</summary>
		public const System.String NAMING_CONTEXT_SYNC_REQ = "2.16.840.1.113719.1.27.100.25";
		
		/// <summary> A constant for the requestPartitionSyncResponse OID.</summary>
		public const System.String NAMING_CONTEXT_SYNC_RES = "2.16.840.1.113719.1.27.100.26";
		
		/// <summary> A constant for the requestSchemaSyncRequest OID.</summary>
		public const System.String SCHEMA_SYNC_REQ = "2.16.840.1.113719.1.27.100.27";
		
		/// <summary> A constant for the requestSchemaSyncResponse OID.</summary>
		public const System.String SCHEMA_SYNC_RES = "2.16.840.1.113719.1.27.100.28";
		
		/// <summary> A constant for the abortPartitionOperationRequest OID.</summary>
		public const System.String ABORT_NAMING_CONTEXT_OP_REQ = "2.16.840.1.113719.1.27.100.29";
		
		/// <summary> A constant for the abortPartitionOperationResponse OID.</summary>
		public const System.String ABORT_NAMING_CONTEXT_OP_RES = "2.16.840.1.113719.1.27.100.30";
		
		/// <summary> A constant for the getContextIdentityNameRequest OID.</summary>
		public const System.String GET_IDENTITY_NAME_REQ = "2.16.840.1.113719.1.27.100.31";
		
		/// <summary> A constant for the getContextIdentityNameResponse OID.</summary>
		public const System.String GET_IDENTITY_NAME_RES = "2.16.840.1.113719.1.27.100.32";
		
		/// <summary> A constant for the getEffectivePrivilegesRequest OID.</summary>
		public const System.String GET_EFFECTIVE_PRIVILEGES_REQ = "2.16.840.1.113719.1.27.100.33";
		
		/// <summary> A constant for the getEffectivePrivilegesResponse OID.</summary>
		public const System.String GET_EFFECTIVE_PRIVILEGES_RES = "2.16.840.1.113719.1.27.100.34";
		
		/// <summary> A constant for the setReplicationFilterRequest OID.</summary>
		public const System.String SET_REPLICATION_FILTER_REQ = "2.16.840.1.113719.1.27.100.35";
		
		/// <summary> A constant for the setReplicationFilterResponse OID.</summary>
		public const System.String SET_REPLICATION_FILTER_RES = "2.16.840.1.113719.1.27.100.36";
		
		/// <summary> A constant for the getReplicationFilterRequest OID.</summary>
		public const System.String GET_REPLICATION_FILTER_REQ = "2.16.840.1.113719.1.27.100.37";
		
		/// <summary> A constant for the getReplicationFilterResponse OID.</summary>
		public const System.String GET_REPLICATION_FILTER_RES = "2.16.840.1.113719.1.27.100.38";
		
		/// <summary> A constant for the splitOrphanPartitionRequest OID.</summary>
		public const System.String CREATE_ORPHAN_NAMING_CONTEXT_REQ = "2.16.840.1.113719.1.27.100.39";
		
		/// <summary> A constant for the splitOrphanPartitionResponse OID.</summary>
		public const System.String CREATE_ORPHAN_NAMING_CONTEXT_RES = "2.16.840.1.113719.1.27.100.40";
		
		/// <summary> A constant for the removeOrphanPartitionRequest OID.</summary>
		public const System.String REMOVE_ORPHAN_NAMING_CONTEXT_REQ = "2.16.840.1.113719.1.27.100.41";
		
		/// <summary> A constant for the removeOrphanPartitionResponse OID.</summary>
		public const System.String REMOVE_ORPHAN_NAMING_CONTEXT_RES = "2.16.840.1.113719.1.27.100.42";
		
		/// <summary> A constant for the triggerBackLinkerRequest OID.</summary>
		public const System.String TRIGGER_BKLINKER_REQ = "2.16.840.1.113719.1.27.100.43";
		
		/// <summary> A constant for the triggerBackLinkerResponse OID.</summary>
		public const System.String TRIGGER_BKLINKER_RES = "2.16.840.1.113719.1.27.100.44";
		
		/// <summary> A constant for the triggerJanitorRequest OID.</summary>
		public const System.String TRIGGER_JANITOR_REQ = "2.16.840.1.113719.1.27.100.47";
		
		/// <summary> A constant for the triggerJanitorResponse OID.</summary>
		public const System.String TRIGGER_JANITOR_RES = "2.16.840.1.113719.1.27.100.48";
		
		/// <summary> A constant for the triggerLimberRequest OID.</summary>
		public const System.String TRIGGER_LIMBER_REQ = "2.16.840.1.113719.1.27.100.49";
		
		/// <summary> A constant for the triggerLimberResponse OID.</summary>
		public const System.String TRIGGER_LIMBER_RES = "2.16.840.1.113719.1.27.100.50";
		
		/// <summary> A constant for the triggerSkulkerRequest OID.</summary>
		public const System.String TRIGGER_SKULKER_REQ = "2.16.840.1.113719.1.27.100.51";
		
		/// <summary> A constant for the triggerSkulkerResponse OID.</summary>
		public const System.String TRIGGER_SKULKER_RES = "2.16.840.1.113719.1.27.100.52";
		
		/// <summary> A constant for the triggerSchemaSyncRequest OID.</summary>
		public const System.String TRIGGER_SCHEMA_SYNC_REQ = "2.16.840.1.113719.1.27.100.53";
		
		/// <summary> A constant for the triggerSchemaSyncResponse OID.</summary>
		public const System.String TRIGGER_SCHEMA_SYNC_RES = "2.16.840.1.113719.1.27.100.54";
		
		/// <summary> A constant for the triggerPartitionPurgeRequest OID.</summary>
		public const System.String TRIGGER_PART_PURGE_REQ = "2.16.840.1.113719.1.27.100.55";
		
		/// <summary> A constant for the triggerPartitionPurgeResponse OID.</summary>
		public const System.String TRIGGER_PART_PURGE_RES = "2.16.840.1.113719.1.27.100.56";
		
		
		/// <summary> A constant that specifies that all servers in a replica ring must be
		/// running for a partition operation to proceed.
		/// </summary>
		public const int Ldap_ENSURE_SERVERS_UP = 1;
		
		
		/// <summary> Identifies this replica as the master replica of the partition.
		/// 
		/// On this type of replica, entries can be modified, and partition
		/// operations can be performed.
		/// </summary>
		public const int Ldap_RT_MASTER = 0;
		
		/// <summary> Identifies this replica as a secondary replica of the partition.
		/// 
		/// On this type of replica, read and write operations can be performed,
		/// and entries can be modified.
		/// </summary>
		public const int Ldap_RT_SECONDARY = 1;
		
		/// <summary> Identifies this replica as a read-only replica of the partition.
		/// 
		/// Only Novell eDirectory synchronization processes can modified
		/// entries on this replica.
		/// </summary>
		public const int Ldap_RT_READONLY = 2;
		
		/// <summary> Identifies this replica as a subordinate reference replica of the
		/// partition.
		/// 
		/// NOvell eDirectory automatically adds these replicas to a server
		/// when the server does not contain replicas of all child partitions.
		/// Only eDirectory can modify information on these types of replicas.
		/// </summary>
		public const int Ldap_RT_SUBREF = 3;
		
		/// <summary> Identifies this replica as a read/write replica of the partition,
		/// but the replica contains sparse data.
		/// 
		/// The replica has been configured to contain only specified object types
		/// and attributes. On this type of replica, only the attributes and objects
		/// contained in the sparse data can be modified.
		/// </summary>
		public const int Ldap_RT_SPARSE_WRITE = 4;
		
		/// <summary> Identifies this replica as a read-only replica of the partition,
		/// but the replica contains sparse data.
		/// 
		/// The replica has been configured to contain only specified object types
		/// and attributes. On this type of replica, only Novell eDirectory
		/// synchronization processes can modify the sparse data.
		/// </summary>
		public const int Ldap_RT_SPARSE_READ = 5;
		
		//Replica States
		
		/// <summary> Indicates that the replica is fully functioning and capable of responding
		/// to requests.
		/// </summary>
		public const int Ldap_RS_ON = 0;
		
		/// <summary> Indicates that a new replica has been added but has not received a full
		/// download of information from the replica ring.
		/// </summary>
		public const int Ldap_RS_NEW_REPLICA = 1;
		
		/// <summary> Indicates that the replica is being deleted and that the request has
		/// been received.
		/// </summary>
		public const int Ldap_RS_DYING_REPLICA = 2;
		
		/// <summary> Indicates that the replica is locked. The move operation uses this state
		/// to lock the parent partition of the child partition that is moving.
		/// </summary>
		public const int Ldap_RS_LOCKED = 3;
		
		/// <summary> Indicates that a new replica has finished receiving its download from the
		/// master replica and is now receiving synchronization updates from other
		/// replicas.
		/// </summary>
		public const int Ldap_RS_TRANSITION_ON = 6;
		
		
		/// <summary> Indicates that the dying replica needs to synchronize with another replica
		/// before being converted either to an external reference, if a root replica,
		/// or to a subordinate reference, if a non-root replica.
		/// </summary>
		public const int Ldap_RS_DEAD_REPLICA = 7;
		
		/// <summary> Indicates that the subordinate references of the new replica are being
		/// added.
		/// </summary>
		public const int Ldap_RS_BEGIN_ADD = 8;
		
		/// <summary> Indicates that a partition is receiving a new master replica.
		/// 
		/// The replica that will be the new master replica is set to this state.
		/// </summary>
		public const int Ldap_RS_MASTER_START = 11;
		
		/// <summary> Indicates that a partition has a new master replica.
		/// 
		/// When the new master is set to this state, Novell eDirectory knows
		/// that the replica is now the master and changes its replica type to
		/// master and the old master to read/write.
		/// </summary>
		public const int Ldap_RS_MASTER_DONE = 12;
		
		/// <summary> Indicates that the partition is going to split into two partitions.
		/// 
		/// In this state, other replicas of the partition are informed of the
		/// pending split.
		/// </summary>
		public const int Ldap_RS_SS_0 = 48; // Replica splitting 0
		
		/// <summary> Indicates that that the split partition operation has started.
		/// 
		/// When the split is finished, the state will change to RS_ON.
		/// </summary>
		public const int Ldap_RS_SS_1 = 49; // Replica splitting 1
		
		/// <summary> Indicates that that two partitions are in the process of joining
		/// into one partition.
		/// 
		/// In this state, the replicas that are affected are informed of the join
		/// operation. The master replica of the parent and child partitions are
		/// first set to this state and then all the replicas of the parent and child.
		/// New replicas are added where needed.
		/// </summary>
		public const int Ldap_RS_JS_0 = 64; // Replica joining 0
		
		/// <summary> Indicates that that two partitions are in the process of joining
		/// into one partition.
		/// 
		/// This state indicates that the join operation is waiting for the new
		/// replicas to synchronize and move to the RS_ON state.
		/// </summary>
		public const int Ldap_RS_JS_1 = 65; // Replica joining 1
		
		/// <summary> Indicates that that two partitions are in the process of joining
		/// into one partition.
		/// 
		/// This state indicates that all the new replicas are in the RS_ON state
		/// and that the rest of the work can be completed.
		/// </summary>
		public const int Ldap_RS_JS_2 = 66; // Replica joining 2
		
		
		// Values for flags used in the replica info class structure
		
		/// <summary> Indicates that the replica is involved with a partition operation,
		/// for example, merging a tree or moving a subtree.
		/// </summary>
		public const int Ldap_DS_FLAG_BUSY = 0x0001;
		
		/// <summary> Indicates that this partition is on the DNS federation boundary.
		/// This flag is only set on DNS trees.
		/// </summary>
		public const int Ldap_DS_FLAG_BOUNDARY = 0x0002;
		
		
		public ReplicationConstants()
		{
		}
	}
}
