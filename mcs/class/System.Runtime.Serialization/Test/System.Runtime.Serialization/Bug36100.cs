using System;
using global::System.Runtime.Serialization;
using global::System.Diagnostics;
using global::System.ServiceModel;
using System.IO;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class Bug36100
	{
		// This test exposed an issue with our dynamic serializer support and
		// would cause problems with static compilation on 64 bit devices in
		// FullAOT mode.
		[Test]
		public void SerializerDynamicInvoke ()
		{
			var a = new DingusSyncData ();
			a.Aircraft = new AircraftDTO[] { new AircraftDTO () { } };
			a.AircraftTypes = new AircraftTypeDTO[] { new AircraftTypeDTO () };
			a.Airlines= new AirlineDTO[] { new AirlineDTO () };
			a.Airports= new AirportDTO[] { new AirportDTO() };
			a.Approaches= new ApproachDTO[] { new ApproachDTO() };
			a.ApproachesLegs= new ApproachesLegDTO[] { new ApproachesLegDTO() };
			a.Binaries= new BinaryCatalogDTO[] { new BinaryCatalogDTO() };
			a.Crews= new CrewDTO[] { new CrewDTO() };
			a.Days= new DayDTO[] { new DayDTO() };
			a.EmploymentEvents= new EmploymentEventDTO[] { new EmploymentEventDTO() };
			a.Events= new EventDTO[] { new EventDTO() };
			a.FlightDataInspection = new DataInspection ();
			a.GlobalSettings= new GlobalSettingDTO[] { new GlobalSettingDTO() };
			a.Hotels= new HotelDTO[] { new HotelDTO() };
			a.Legs= new LegDTO[] { new LegDTO() };
			a.Notes= new NoteDTO[] { new NoteDTO() };
			a.PayperiodEvents= new PayperiodEventDTO[] { new PayperiodEventDTO() };
			a.PayrollCategories= new PayrollCategoryDTO[] { new PayrollCategoryDTO() };
			a.Payrolls= new PayrollDTO[] { new PayrollDTO() };
			a.Performances= new PerformanceDTO[] { new PerformanceDTO() };
			a.Positions= new PositionDTO[] { new PositionDTO() };
			a.ReglatoryOperationTypes= new ReglatoryOperationTypeDTO[] { new ReglatoryOperationTypeDTO() };
			a.Trips= new TripDTO[] { new TripDTO() };
			a.UserSettings= new UserSettingDTO[] { new UserSettingDTO() };

			Console.WriteLine ("Size is: {0}", global::System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)));
			using (var ms = new MemoryStream ()) {
				DataContractSerializer serializer = new DataContractSerializer (typeof(DingusSyncData));
				serializer.WriteObject (ms, a);
				ms.Position = 0;
				var b = serializer.ReadObject (ms);
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DingusSyncData", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DingusSyncData : object
	{

		AircraftDTO[] AircraftField;

		AircraftTypeDTO[] AircraftTypesField;

		AirlineDTO[] AirlinesField;

		AirportDTO[] AirportsField;

		ApproachDTO[] ApproachesField;

		ApproachesLegDTO[] ApproachesLegsField;

		BinaryCatalogDTO[] BinariesField;

		CrewDTO[] CrewsField;

		DayDTO[] DaysField;

		EmploymentEventDTO[] EmploymentEventsField;

		EventDTO[] EventsField;

		DataInspection FlightDataInspectionField;

		GlobalSettingDTO[] GlobalSettingsField;

		HotelDTO[] HotelsField;

		LegDTO[] LegsField;

		NoteDTO[] NotesField;

		PayperiodEventDTO[] PayperiodEventsField;

		PayrollCategoryDTO[] PayrollCategoriesField;

		PayrollDTO[] PayrollsField;

		PerformanceDTO[] PerformancesField;

		PositionDTO[] PositionsField;

		ReglatoryOperationTypeDTO[] ReglatoryOperationTypesField;

		TripDTO[] TripsField;

		UserSettingDTO[] UserSettingsField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AircraftDTO[] Aircraft
		{
			get
			{
				return this.AircraftField;
			}
			set
			{
				this.AircraftField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AircraftTypeDTO[] AircraftTypes
		{
			get
			{
				return this.AircraftTypesField;
			}
			set
			{
				this.AircraftTypesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AirlineDTO[] Airlines
		{
			get
			{
				return this.AirlinesField;
			}
			set
			{
				this.AirlinesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AirportDTO[] Airports
		{
			get
			{
				return this.AirportsField;
			}
			set
			{
				this.AirportsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ApproachDTO[] Approaches
		{
			get
			{
				return this.ApproachesField;
			}
			set
			{
				this.ApproachesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ApproachesLegDTO[] ApproachesLegs
		{
			get
			{
				return this.ApproachesLegsField;
			}
			set
			{
				this.ApproachesLegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public BinaryCatalogDTO[] Binaries
		{
			get
			{
				return this.BinariesField;
			}
			set
			{
				this.BinariesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public CrewDTO[] Crews
		{
			get
			{
				return this.CrewsField;
			}
			set
			{
				this.CrewsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DayDTO[] Days
		{
			get
			{
				return this.DaysField;
			}
			set
			{
				this.DaysField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EmploymentEventDTO[] EmploymentEvents
		{
			get
			{
				return this.EmploymentEventsField;
			}
			set
			{
				this.EmploymentEventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EventDTO[] Events
		{
			get
			{
				return this.EventsField;
			}
			set
			{
				this.EventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DataInspection FlightDataInspection
		{
			get
			{
				return this.FlightDataInspectionField;
			}
			set
			{
				this.FlightDataInspectionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public GlobalSettingDTO[] GlobalSettings
		{
			get
			{
				return this.GlobalSettingsField;
			}
			set
			{
				this.GlobalSettingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public HotelDTO[] Hotels
		{
			get
			{
				return this.HotelsField;
			}
			set
			{
				this.HotelsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public LegDTO[] Legs
		{
			get
			{
				return this.LegsField;
			}
			set
			{
				this.LegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public NoteDTO[] Notes
		{
			get
			{
				return this.NotesField;
			}
			set
			{
				this.NotesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayperiodEventDTO[] PayperiodEvents
		{
			get
			{
				return this.PayperiodEventsField;
			}
			set
			{
				this.PayperiodEventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayrollCategoryDTO[] PayrollCategories
		{
			get
			{
				return this.PayrollCategoriesField;
			}
			set
			{
				this.PayrollCategoriesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayrollDTO[] Payrolls
		{
			get
			{
				return this.PayrollsField;
			}
			set
			{
				this.PayrollsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PerformanceDTO[] Performances
		{
			get
			{
				return this.PerformancesField;
			}
			set
			{
				this.PerformancesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PositionDTO[] Positions
		{
			get
			{
				return this.PositionsField;
			}
			set
			{
				this.PositionsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ReglatoryOperationTypeDTO[] ReglatoryOperationTypes
		{
			get
			{
				return this.ReglatoryOperationTypesField;
			}
			set
			{
				this.ReglatoryOperationTypesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public TripDTO[] Trips
		{
			get
			{
				return this.TripsField;
			}
			set
			{
				this.TripsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public UserSettingDTO[] UserSettings
		{
			get
			{
				return this.UserSettingsField;
			}
			set
			{
				this.UserSettingsField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DataInspection", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DataInspection : object
	{

		private int DayCountField;

		private int LegCountField;

		private Nullable<global::System.DateTime> MaxTripSequenceEndField;

		private Nullable<global::System.DateTime> MinTripSequenceStartField;

		private int TripCountField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int DayCount
		{
			get
			{
				return this.DayCountField;
			}
			set
			{
				this.DayCountField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int LegCount
		{
			get
			{
				return this.LegCountField;
			}
			set
			{
				this.LegCountField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> MaxTripSequenceEnd
		{
			get
			{
				return this.MaxTripSequenceEndField;
			}
			set
			{
				this.MaxTripSequenceEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> MinTripSequenceStart
		{
			get
			{
				return this.MinTripSequenceStartField;
			}
			set
			{
				this.MinTripSequenceStartField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TripCount
		{
			get
			{
				return this.TripCountField;
			}
			set
			{
				this.TripCountField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AircraftDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AircraftDTO : object
	{

		private string aircraftIdField;

		private string aircraftTypeIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string currentAirlineIdField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<decimal> payrateField;

		private Nullable<bool> previewField;

		private string previousAirlineIdField;

		private string registrationField;

		private string shipNumberField;

		private Nullable<bool> syncedField;

		private string tailField;

		private Nullable<bool> usePayrateField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftId
		{
			get
			{
				return this.aircraftIdField;
			}
			set
			{
				this.aircraftIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftTypeId
		{
			get
			{
				return this.aircraftTypeIdField;
			}
			set
			{
				this.aircraftTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string currentAirlineId
		{
			get
			{
				return this.currentAirlineIdField;
			}
			set
			{
				this.currentAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> payrate
		{
			get
			{
				return this.payrateField;
			}
			set
			{
				this.payrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string previousAirlineId
		{
			get
			{
				return this.previousAirlineIdField;
			}
			set
			{
				this.previousAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string registration
		{
			get
			{
				return this.registrationField;
			}
			set
			{
				this.registrationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string shipNumber
		{
			get
			{
				return this.shipNumberField;
			}
			set
			{
				this.shipNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tail
		{
			get
			{
				return this.tailField;
			}
			set
			{
				this.tailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> usePayrate
		{
			get
			{
				return this.usePayrateField;
			}
			set
			{
				this.usePayrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AircraftTypeDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AircraftTypeDTO : object
	{

		private string aircraftTypeIdField;

		private string aselField;

		private string configField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string iconUrlField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private string transportField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftTypeId
		{
			get
			{
				return this.aircraftTypeIdField;
			}
			set
			{
				this.aircraftTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string asel
		{
			get
			{
				return this.aselField;
			}
			set
			{
				this.aselField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string config
		{
			get
			{
				return this.configField;
			}
			set
			{
				this.configField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string iconUrl
		{
			get
			{
				return this.iconUrlField;
			}
			set
			{
				this.iconUrlField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string transport
		{
			get
			{
				return this.transportField;
			}
			set
			{
				this.transportField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AirlineDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AirlineDTO : object
	{

		private string airlineIdField;

		private string airlineNameField;

		private string callSignField;

		private string countryField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string icaoField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string phoneField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineName
		{
			get
			{
				return this.airlineNameField;
			}
			set
			{
				this.airlineNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string callSign
		{
			get
			{
				return this.callSignField;
			}
			set
			{
				this.callSignField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string country
		{
			get
			{
				return this.countryField;
			}
			set
			{
				this.countryField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string icao
		{
			get
			{
				return this.icaoField;
			}
			set
			{
				this.icaoField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AirportDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AirportDTO : object
	{

		private string airlineNameField;

		private string airportIdField;

		private string airportNameField;

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<bool> dstField;

		private string emailField;

		private string faaField;

		private string iataField;

		private string icaoField;

		private string idField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<decimal> latitudeField;

		private string localityField;

		private string locationField;

		private Nullable<decimal> longitudeField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string olsentimezonenameField;

		private string phoneField;

		private string pictureField;

		private Nullable<bool> previewField;

		private Nullable<int> privacyLevelField;

		private string regionField;

		private Nullable<bool> syncedField;

		private int userIdField;

		private Nullable<decimal> utcoffsetField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineName
		{
			get
			{
				return this.airlineNameField;
			}
			set
			{
				this.airlineNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airportId
		{
			get
			{
				return this.airportIdField;
			}
			set
			{
				this.airportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airportName
		{
			get
			{
				return this.airportNameField;
			}
			set
			{
				this.airportNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> dst
		{
			get
			{
				return this.dstField;
			}
			set
			{
				this.dstField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string faa
		{
			get
			{
				return this.faaField;
			}
			set
			{
				this.faaField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string iata
		{
			get
			{
				return this.iataField;
			}
			set
			{
				this.iataField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string icao
		{
			get
			{
				return this.icaoField;
			}
			set
			{
				this.icaoField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string id
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> latitude
		{
			get
			{
				return this.latitudeField;
			}
			set
			{
				this.latitudeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string locality
		{
			get
			{
				return this.localityField;
			}
			set
			{
				this.localityField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> longitude
		{
			get
			{
				return this.longitudeField;
			}
			set
			{
				this.longitudeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string olsentimezonename
		{
			get
			{
				return this.olsentimezonenameField;
			}
			set
			{
				this.olsentimezonenameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string region
		{
			get
			{
				return this.regionField;
			}
			set
			{
				this.regionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> utcoffset
		{
			get
			{
				return this.utcoffsetField;
			}
			set
			{
				this.utcoffsetField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ApproachDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ApproachDTO : object
	{

		private string approachIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private string typeField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ApproachesLegDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ApproachesLegDTO : object
	{

		private string approachIdField;

		private string approachesLegsIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string legIdField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachesLegsId
		{
			get
			{
				return this.approachesLegsIdField;
			}
			set
			{
				this.approachesLegsIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string legId
		{
			get
			{
				return this.legIdField;
			}
			set
			{
				this.legIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="BinaryCatalogDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class BinaryCatalogDTO : object
	{

		private global::System.Guid RowGuidField;

		private Nullable<int> areaIdField;

		private string binaryCatalogIdField;

		private Nullable<int> contentLengthField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string filenameField;

		private string folderIdField;

		private Nullable<bool> isSecuredField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<global::System.DateTime> lastWriteTimeUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private string titleField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.Guid RowGuid
		{
			get
			{
				return this.RowGuidField;
			}
			set
			{
				this.RowGuidField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> areaId
		{
			get
			{
				return this.areaIdField;
			}
			set
			{
				this.areaIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string binaryCatalogId
		{
			get
			{
				return this.binaryCatalogIdField;
			}
			set
			{
				this.binaryCatalogIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> contentLength
		{
			get
			{
				return this.contentLengthField;
			}
			set
			{
				this.contentLengthField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string filename
		{
			get
			{
				return this.filenameField;
			}
			set
			{
				this.filenameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string folderId
		{
			get
			{
				return this.folderIdField;
			}
			set
			{
				this.folderIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> isSecured
		{
			get
			{
				return this.isSecuredField;
			}
			set
			{
				this.isSecuredField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastWriteTimeUtc
		{
			get
			{
				return this.lastWriteTimeUtcField;
			}
			set
			{
				this.lastWriteTimeUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string title
		{
			get
			{
				return this.titleField;
			}
			set
			{
				this.titleField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="CrewDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class CrewDTO : object
	{

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string crewIdField;

		private string crewNameField;

		private string currentAirlineIdField;

		private Nullable<bool> deletedField;

		private string emailField;

		private string facebookField;

		private string idField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string locationField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string phoneField;

		private string pictureField;

		private string positionField;

		private Nullable<bool> previewField;

		private string previousAirlineIdField;

		private Nullable<int> privacyLevelField;

		private Nullable<bool> syncedField;

		private string twitterField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string crewId
		{
			get
			{
				return this.crewIdField;
			}
			set
			{
				this.crewIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string crewName
		{
			get
			{
				return this.crewNameField;
			}
			set
			{
				this.crewNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string currentAirlineId
		{
			get
			{
				return this.currentAirlineIdField;
			}
			set
			{
				this.currentAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string facebook
		{
			get
			{
				return this.facebookField;
			}
			set
			{
				this.facebookField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string id
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string position
		{
			get
			{
				return this.positionField;
			}
			set
			{
				this.positionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string previousAirlineId
		{
			get
			{
				return this.previousAirlineIdField;
			}
			set
			{
				this.previousAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string twitter
		{
			get
			{
				return this.twitterField;
			}
			set
			{
				this.twitterField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DayDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DayDTO : object
	{

		private Nullable<int> blockField;

		private string calendarIdentifierField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<int> creditField;

		private string dayIdField;

		private Nullable<bool> deletedField;

		private Nullable<decimal> dutyField;

		private Nullable<global::System.DateTime> dutyOffField;

		private Nullable<global::System.DateTime> dutyOnField;

		private Nullable<int> fDPField;

		private Nullable<global::System.DateTime> fDPEndTimeField;

		private Nullable<decimal> flightTimeField;

		private Nullable<int> grossPayField;

		private string hotelIdField;

		private Nullable<decimal> instrumentField;

		private Nullable<int> landingsField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<decimal> nightField;

		private Nullable<int> nightLandingsField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> overrideDutyOffField;

		private Nullable<bool> overrideDutyOnField;

		private Nullable<bool> previewField;

		private Nullable<int> rdpField;

		private Nullable<global::System.DateTime> rdpBeginField;

		private Nullable<global::System.DateTime> rdpEndField;

		private Nullable<int> scheduleBlockField;

		private Nullable<int> splitDutyField;

		private Nullable<global::System.DateTime> splitDutyBeginField;

		private Nullable<global::System.DateTime> splitDutyEndField;

		private Nullable<bool> syncedField;

		private string tripIdField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> block
		{
			get
			{
				return this.blockField;
			}
			set
			{
				this.blockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> credit
		{
			get
			{
				return this.creditField;
			}
			set
			{
				this.creditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dayId
		{
			get
			{
				return this.dayIdField;
			}
			set
			{
				this.dayIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> duty
		{
			get
			{
				return this.dutyField;
			}
			set
			{
				this.dutyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dutyOff
		{
			get
			{
				return this.dutyOffField;
			}
			set
			{
				this.dutyOffField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dutyOn
		{
			get
			{
				return this.dutyOnField;
			}
			set
			{
				this.dutyOnField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> fDP
		{
			get
			{
				return this.fDPField;
			}
			set
			{
				this.fDPField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> fDPEndTime
		{
			get
			{
				return this.fDPEndTimeField;
			}
			set
			{
				this.fDPEndTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> flightTime
		{
			get
			{
				return this.flightTimeField;
			}
			set
			{
				this.flightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> grossPay
		{
			get
			{
				return this.grossPayField;
			}
			set
			{
				this.grossPayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelId
		{
			get
			{
				return this.hotelIdField;
			}
			set
			{
				this.hotelIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> instrument
		{
			get
			{
				return this.instrumentField;
			}
			set
			{
				this.instrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> landings
		{
			get
			{
				return this.landingsField;
			}
			set
			{
				this.landingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> night
		{
			get
			{
				return this.nightField;
			}
			set
			{
				this.nightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> nightLandings
		{
			get
			{
				return this.nightLandingsField;
			}
			set
			{
				this.nightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overrideDutyOff
		{
			get
			{
				return this.overrideDutyOffField;
			}
			set
			{
				this.overrideDutyOffField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overrideDutyOn
		{
			get
			{
				return this.overrideDutyOnField;
			}
			set
			{
				this.overrideDutyOnField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rdp
		{
			get
			{
				return this.rdpField;
			}
			set
			{
				this.rdpField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> rdpBegin
		{
			get
			{
				return this.rdpBeginField;
			}
			set
			{
				this.rdpBeginField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> rdpEnd
		{
			get
			{
				return this.rdpEndField;
			}
			set
			{
				this.rdpEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> scheduleBlock
		{
			get
			{
				return this.scheduleBlockField;
			}
			set
			{
				this.scheduleBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> splitDuty
		{
			get
			{
				return this.splitDutyField;
			}
			set
			{
				this.splitDutyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> splitDutyBegin
		{
			get
			{
				return this.splitDutyBeginField;
			}
			set
			{
				this.splitDutyBeginField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> splitDutyEnd
		{
			get
			{
				return this.splitDutyEndField;
			}
			set
			{
				this.splitDutyEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripId
		{
			get
			{
				return this.tripIdField;
			}
			set
			{
				this.tripIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EmploymentEventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EmploymentEventDTO : object
	{

		private string airlineIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string employmentEventIdField;

		private Nullable<global::System.DateTime> firstDateField;

		private Nullable<global::System.DateTime> lastDateField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string employmentEventId
		{
			get
			{
				return this.employmentEventIdField;
			}
			set
			{
				this.employmentEventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> firstDate
		{
			get
			{
				return this.firstDateField;
			}
			set
			{
				this.firstDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastDate
		{
			get
			{
				return this.lastDateField;
			}
			set
			{
				this.lastDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EventDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<global::System.DateTime> dateRangeField;

		private Nullable<bool> deletedField;

		private string eventIdField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<int> previewField;

		private Nullable<bool> syncedField;

		private string urlField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dateRange
		{
			get
			{
				return this.dateRangeField;
			}
			set
			{
				this.dateRangeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string eventId
		{
			get
			{
				return this.eventIdField;
			}
			set
			{
				this.eventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string url
		{
			get
			{
				return this.urlField;
			}
			set
			{
				this.urlField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="GlobalSettingDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class GlobalSettingDTO : object
	{

		private string DescriptionField;

		private global::System.Guid GlobalSettingIdField;

		private global::System.DateTime LastUpdatedUtcField;

		private string SettingKeyField;

		private string SettingValueField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Description
		{
			get
			{
				return this.DescriptionField;
			}
			set
			{
				this.DescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.Guid GlobalSettingId
		{
			get
			{
				return this.GlobalSettingIdField;
			}
			set
			{
				this.GlobalSettingIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime LastUpdatedUtc
		{
			get
			{
				return this.LastUpdatedUtcField;
			}
			set
			{
				this.LastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string SettingKey
		{
			get
			{
				return this.SettingKeyField;
			}
			set
			{
				this.SettingKeyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string SettingValue
		{
			get
			{
				return this.SettingValueField;
			}
			set
			{
				this.SettingValueField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="HotelDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class HotelDTO : object
	{

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string emailField;

		private string hotelIdField;

		private string hotelNameField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string locationField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string numberField;

		private string phoneField;

		private string pictureField;

		private Nullable<bool> previewField;

		private Nullable<int> privacyLevelField;

		private Nullable<int> ratingsField;

		private string sharedNotesField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelId
		{
			get
			{
				return this.hotelIdField;
			}
			set
			{
				this.hotelIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelName
		{
			get
			{
				return this.hotelNameField;
			}
			set
			{
				this.hotelNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string number
		{
			get
			{
				return this.numberField;
			}
			set
			{
				this.numberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> ratings
		{
			get
			{
				return this.ratingsField;
			}
			set
			{
				this.ratingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string sharedNotes
		{
			get
			{
				return this.sharedNotesField;
			}
			set
			{
				this.sharedNotesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="LegDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class LegDTO : object
	{

		private Nullable<int> actualBlockField;

		private string aircraftIdField;

		private string approachIdField;

		private string cabinCrewAIdField;

		private string cabinCrewBIdField;

		private string calendarIdentifierField;

		private string captainIdField;

		private Nullable<bool> completedField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string dayIdField;

		private Nullable<bool> deletedField;

		private string departureAirportIdField;

		private string deptField;

		private string deptGateField;

		private string destField;

		private string destGateField;

		private string destinationAirportIdField;

		private Nullable<global::System.DateTime> etaField;

		private string firstOfficerIdField;

		private string flightNumberField;

		private Nullable<int> flightTimeField;

		private Nullable<global::System.DateTime> inOOOIField;

		private Nullable<decimal> instrumentField;

		private Nullable<int> landingsField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string legIdField;

		private Nullable<int> modelVersionField;

		private Nullable<decimal> nightField;

		private Nullable<int> nightLandingsField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<global::System.DateTime> offOOOIField;

		private Nullable<global::System.DateTime> onOOOIField;

		private string operationTypeIdField;

		private string otherCrewAIdField;

		private string otherCrewBIdField;

		private Nullable<global::System.DateTime> outOOOIField;

		private string payIdField;

		private string positionIdField;

		private Nullable<bool> previewField;

		private string registrationField;

		private string remarksField;

		private Nullable<int> scheduledBlockField;

		private Nullable<global::System.DateTime> scheduledInField;

		private Nullable<global::System.DateTime> scheduledOutField;

		private Nullable<int> sequenceField;

		private Nullable<bool> syncedField;

		private Nullable<int> taxiTimeInField;

		private Nullable<int> taxiTimeOutField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actualBlock
		{
			get
			{
				return this.actualBlockField;
			}
			set
			{
				this.actualBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftId
		{
			get
			{
				return this.aircraftIdField;
			}
			set
			{
				this.aircraftIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string cabinCrewAId
		{
			get
			{
				return this.cabinCrewAIdField;
			}
			set
			{
				this.cabinCrewAIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string cabinCrewBId
		{
			get
			{
				return this.cabinCrewBIdField;
			}
			set
			{
				this.cabinCrewBIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string captainId
		{
			get
			{
				return this.captainIdField;
			}
			set
			{
				this.captainIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> completed
		{
			get
			{
				return this.completedField;
			}
			set
			{
				this.completedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dayId
		{
			get
			{
				return this.dayIdField;
			}
			set
			{
				this.dayIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string departureAirportId
		{
			get
			{
				return this.departureAirportIdField;
			}
			set
			{
				this.departureAirportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dept
		{
			get
			{
				return this.deptField;
			}
			set
			{
				this.deptField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string deptGate
		{
			get
			{
				return this.deptGateField;
			}
			set
			{
				this.deptGateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dest
		{
			get
			{
				return this.destField;
			}
			set
			{
				this.destField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string destGate
		{
			get
			{
				return this.destGateField;
			}
			set
			{
				this.destGateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string destinationAirportId
		{
			get
			{
				return this.destinationAirportIdField;
			}
			set
			{
				this.destinationAirportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> eta
		{
			get
			{
				return this.etaField;
			}
			set
			{
				this.etaField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string firstOfficerId
		{
			get
			{
				return this.firstOfficerIdField;
			}
			set
			{
				this.firstOfficerIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string flightNumber
		{
			get
			{
				return this.flightNumberField;
			}
			set
			{
				this.flightNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> flightTime
		{
			get
			{
				return this.flightTimeField;
			}
			set
			{
				this.flightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> inOOOI
		{
			get
			{
				return this.inOOOIField;
			}
			set
			{
				this.inOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> instrument
		{
			get
			{
				return this.instrumentField;
			}
			set
			{
				this.instrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> landings
		{
			get
			{
				return this.landingsField;
			}
			set
			{
				this.landingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string legId
		{
			get
			{
				return this.legIdField;
			}
			set
			{
				this.legIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> night
		{
			get
			{
				return this.nightField;
			}
			set
			{
				this.nightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> nightLandings
		{
			get
			{
				return this.nightLandingsField;
			}
			set
			{
				this.nightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> offOOOI
		{
			get
			{
				return this.offOOOIField;
			}
			set
			{
				this.offOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> onOOOI
		{
			get
			{
				return this.onOOOIField;
			}
			set
			{
				this.onOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationTypeId
		{
			get
			{
				return this.operationTypeIdField;
			}
			set
			{
				this.operationTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string otherCrewAId
		{
			get
			{
				return this.otherCrewAIdField;
			}
			set
			{
				this.otherCrewAIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string otherCrewBId
		{
			get
			{
				return this.otherCrewBIdField;
			}
			set
			{
				this.otherCrewBIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> outOOOI
		{
			get
			{
				return this.outOOOIField;
			}
			set
			{
				this.outOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payId
		{
			get
			{
				return this.payIdField;
			}
			set
			{
				this.payIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string positionId
		{
			get
			{
				return this.positionIdField;
			}
			set
			{
				this.positionIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string registration
		{
			get
			{
				return this.registrationField;
			}
			set
			{
				this.registrationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string remarks
		{
			get
			{
				return this.remarksField;
			}
			set
			{
				this.remarksField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> scheduledBlock
		{
			get
			{
				return this.scheduledBlockField;
			}
			set
			{
				this.scheduledBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> scheduledIn
		{
			get
			{
				return this.scheduledInField;
			}
			set
			{
				this.scheduledInField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> scheduledOut
		{
			get
			{
				return this.scheduledOutField;
			}
			set
			{
				this.scheduledOutField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> sequence
		{
			get
			{
				return this.sequenceField;
			}
			set
			{
				this.sequenceField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> taxiTimeIn
		{
			get
			{
				return this.taxiTimeInField;
			}
			set
			{
				this.taxiTimeInField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> taxiTimeOut
		{
			get
			{
				return this.taxiTimeOutField;
			}
			set
			{
				this.taxiTimeOutField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="NoteDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class NoteDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string note1Field;

		private string noteIdField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string note1
		{
			get
			{
				return this.note1Field;
			}
			set
			{
				this.note1Field = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string noteId
		{
			get
			{
				return this.noteIdField;
			}
			set
			{
				this.noteIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayperiodEventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayperiodEventDTO : object
	{

		private string airlineIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> firstDateField;

		private Nullable<global::System.DateTime> lastDateField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string payperiodEventIdField;

		private string periodDescriptionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> firstDate
		{
			get
			{
				return this.firstDateField;
			}
			set
			{
				this.firstDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastDate
		{
			get
			{
				return this.lastDateField;
			}
			set
			{
				this.lastDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payperiodEventId
		{
			get
			{
				return this.payperiodEventIdField;
			}
			set
			{
				this.payperiodEventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string periodDescription
		{
			get
			{
				return this.periodDescriptionField;
			}
			set
			{
				this.periodDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayrollCategoryDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayrollCategoryDTO : object
	{

		private Nullable<bool> aboveGuaranteeField;

		private Nullable<bool> applyRigField;

		private Nullable<bool> applyToFlightTimeField;

		private Nullable<bool> applyToLegalityField;

		private Nullable<bool> applyToPayField;

		private Nullable<bool> copyLegField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> minimumCreditField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> overridePayField;

		private Nullable<decimal> payrateField;

		private string payrollCategoriesIdField;

		private string plainDescriptionField;

		private Nullable<bool> previewField;

		private Nullable<int> rigAField;

		private Nullable<int> rigBField;

		private Nullable<bool> selectableField;

		private Nullable<bool> setAllLegsField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> aboveGuarantee
		{
			get
			{
				return this.aboveGuaranteeField;
			}
			set
			{
				this.aboveGuaranteeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyRig
		{
			get
			{
				return this.applyRigField;
			}
			set
			{
				this.applyRigField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToFlightTime
		{
			get
			{
				return this.applyToFlightTimeField;
			}
			set
			{
				this.applyToFlightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToLegality
		{
			get
			{
				return this.applyToLegalityField;
			}
			set
			{
				this.applyToLegalityField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToPay
		{
			get
			{
				return this.applyToPayField;
			}
			set
			{
				this.applyToPayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> copyLeg
		{
			get
			{
				return this.copyLegField;
			}
			set
			{
				this.copyLegField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> minimumCredit
		{
			get
			{
				return this.minimumCreditField;
			}
			set
			{
				this.minimumCreditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overridePay
		{
			get
			{
				return this.overridePayField;
			}
			set
			{
				this.overridePayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> payrate
		{
			get
			{
				return this.payrateField;
			}
			set
			{
				this.payrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollCategoriesId
		{
			get
			{
				return this.payrollCategoriesIdField;
			}
			set
			{
				this.payrollCategoriesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string plainDescription
		{
			get
			{
				return this.plainDescriptionField;
			}
			set
			{
				this.plainDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rigA
		{
			get
			{
				return this.rigAField;
			}
			set
			{
				this.rigAField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rigB
		{
			get
			{
				return this.rigBField;
			}
			set
			{
				this.rigBField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> setAllLegs
		{
			get
			{
				return this.setAllLegsField;
			}
			set
			{
				this.setAllLegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayrollDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayrollDTO : object
	{

		private Nullable<int> actualField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<int> creditField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string payrollCategoriesIdField;

		private string payrollIdField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actual
		{
			get
			{
				return this.actualField;
			}
			set
			{
				this.actualField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> credit
		{
			get
			{
				return this.creditField;
			}
			set
			{
				this.creditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollCategoriesId
		{
			get
			{
				return this.payrollCategoriesIdField;
			}
			set
			{
				this.payrollCategoriesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollId
		{
			get
			{
				return this.payrollIdField;
			}
			set
			{
				this.payrollIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PerformanceDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PerformanceDTO : object
	{

		private Nullable<int> actualField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<int> deviatedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string performanceIdField;

		private Nullable<int> plannedFuelField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actual
		{
			get
			{
				return this.actualField;
			}
			set
			{
				this.actualField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> deviated
		{
			get
			{
				return this.deviatedField;
			}
			set
			{
				this.deviatedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string performanceId
		{
			get
			{
				return this.performanceIdField;
			}
			set
			{
				this.performanceIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> plannedFuel
		{
			get
			{
				return this.plannedFuelField;
			}
			set
			{
				this.plannedFuelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PositionDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PositionDTO : object
	{

		private Nullable<bool> autoNightLandingField;

		private Nullable<bool> checkAirmanField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> creditLandingField;

		private Nullable<bool> deletedField;

		private Nullable<bool> ioeField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> pilotFlyingField;

		private Nullable<bool> pilotInCommandField;

		private string position1Field;

		private string positionIdField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> autoNightLanding
		{
			get
			{
				return this.autoNightLandingField;
			}
			set
			{
				this.autoNightLandingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> checkAirman
		{
			get
			{
				return this.checkAirmanField;
			}
			set
			{
				this.checkAirmanField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> creditLanding
		{
			get
			{
				return this.creditLandingField;
			}
			set
			{
				this.creditLandingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> ioe
		{
			get
			{
				return this.ioeField;
			}
			set
			{
				this.ioeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> pilotFlying
		{
			get
			{
				return this.pilotFlyingField;
			}
			set
			{
				this.pilotFlyingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> pilotInCommand
		{
			get
			{
				return this.pilotInCommandField;
			}
			set
			{
				this.pilotInCommandField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string position1
		{
			get
			{
				return this.position1Field;
			}
			set
			{
				this.position1Field = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string positionId
		{
			get
			{
				return this.positionIdField;
			}
			set
			{
				this.positionIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ReglatoryOperationTypeDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ReglatoryOperationTypeDTO : object
	{

		private Nullable<bool> activeField;

		private Nullable<bool> canMixOperationsField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string operationAbvreviationField;

		private string operationDescriptionField;

		private Nullable<bool> previewField;

		private string reglatoryOperationTypesIdField;

		private string schemaURLField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> active
		{
			get
			{
				return this.activeField;
			}
			set
			{
				this.activeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> canMixOperations
		{
			get
			{
				return this.canMixOperationsField;
			}
			set
			{
				this.canMixOperationsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationAbvreviation
		{
			get
			{
				return this.operationAbvreviationField;
			}
			set
			{
				this.operationAbvreviationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationDescription
		{
			get
			{
				return this.operationDescriptionField;
			}
			set
			{
				this.operationDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string reglatoryOperationTypesId
		{
			get
			{
				return this.reglatoryOperationTypesIdField;
			}
			set
			{
				this.reglatoryOperationTypesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string schemaURL
		{
			get
			{
				return this.schemaURLField;
			}
			set
			{
				this.schemaURLField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="TripDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class TripDTO : object
	{

		private bool activeField;

		private string calendarIdentifierField;

		private Nullable<bool> completedField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<global::System.DateTime> seqEndTimeField;

		private Nullable<global::System.DateTime> seqStartTimeField;

		private Nullable<bool> syncedField;

		private Nullable<int> tafbField;

		private Nullable<int> totalBlockField;

		private Nullable<int> totalCreditField;

		private Nullable<decimal> totalFlightTimeField;

		private Nullable<decimal> totalInstrumentField;

		private Nullable<int> totalLandingsField;

		private Nullable<decimal> totalNightField;

		private Nullable<int> totalNightLandingsField;

		private Nullable<decimal> totalPayFField;

		private string tripIdField;

		private string tripNumberField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool active
		{
			get
			{
				return this.activeField;
			}
			set
			{
				this.activeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> completed
		{
			get
			{
				return this.completedField;
			}
			set
			{
				this.completedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> seqEndTime
		{
			get
			{
				return this.seqEndTimeField;
			}
			set
			{
				this.seqEndTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> seqStartTime
		{
			get
			{
				return this.seqStartTimeField;
			}
			set
			{
				this.seqStartTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> tafb
		{
			get
			{
				return this.tafbField;
			}
			set
			{
				this.tafbField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalBlock
		{
			get
			{
				return this.totalBlockField;
			}
			set
			{
				this.totalBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalCredit
		{
			get
			{
				return this.totalCreditField;
			}
			set
			{
				this.totalCreditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalFlightTime
		{
			get
			{
				return this.totalFlightTimeField;
			}
			set
			{
				this.totalFlightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalInstrument
		{
			get
			{
				return this.totalInstrumentField;
			}
			set
			{
				this.totalInstrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalLandings
		{
			get
			{
				return this.totalLandingsField;
			}
			set
			{
				this.totalLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalNight
		{
			get
			{
				return this.totalNightField;
			}
			set
			{
				this.totalNightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalNightLandings
		{
			get
			{
				return this.totalNightLandingsField;
			}
			set
			{
				this.totalNightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalPayF
		{
			get
			{
				return this.totalPayFField;
			}
			set
			{
				this.totalPayFField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripId
		{
			get
			{
				return this.tripIdField;
			}
			set
			{
				this.tripIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripNumber
		{
			get
			{
				return this.tripNumberField;
			}
			set
			{
				this.tripNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="UserSettingDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class UserSettingDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string keyField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private string stringValueField;

		private Nullable<bool> syncedField;

		private int userIdField;

		private string userSettingIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string key
		{
			get
			{
				return this.keyField;
			}
			set
			{
				this.keyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string stringValue
		{
			get
			{
				return this.stringValueField;
			}
			set
			{
				this.stringValueField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string userSettingId
		{
			get
			{
				return this.userSettingIdField;
			}
			set
			{
				this.userSettingIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DingusSyncResponse", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DingusSyncResponse : object
	{

		DingusSyncData CloudDataField;

		private string StatusField;

		private bool SuccessField;

		private global::System.DateTime SyncDateLineField;

		private long SyncDurationField;

		private global::System.DateTime SyncEndedField;

		private global::System.DateTime SyncStartedField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DingusSyncData CloudData
		{
			get
			{
				return this.CloudDataField;
			}
			set
			{
				this.CloudDataField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Status
		{
			get
			{
				return this.StatusField;
			}
			set
			{
				this.StatusField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool Success
		{
			get
			{
				return this.SuccessField;
			}
			set
			{
				this.SuccessField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncDateLine
		{
			get
			{
				return this.SyncDateLineField;
			}
			set
			{
				this.SyncDateLineField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public long SyncDuration
		{
			get
			{
				return this.SyncDurationField;
			}
			set
			{
				this.SyncDurationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncEnded
		{
			get
			{
				return this.SyncEndedField;
			}
			set
			{
				this.SyncEndedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncStarted
		{
			get
			{
				return this.SyncStartedField;
			}
			set
			{
				this.SyncStartedField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="BinaryTransferResponse", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class BinaryTransferResponse : object
	{

		private string ErrorMessageField;

		private bool SuccessField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string ErrorMessage
		{
			get
			{
				return this.ErrorMessageField;
			}
			set
			{
				this.ErrorMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool Success
		{
			get
			{
				return this.SuccessField;
			}
			set
			{
				this.SuccessField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="SyncStatus", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class SyncStatus : object
	{

		EntitySyncState[] SyncStateField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EntitySyncState[] SyncState
		{
			get
			{
				return this.SyncStateField;
			}
			set
			{
				this.SyncStateField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EntitySyncState", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EntitySyncState : object
	{

		private string EntityNameField;

		private Nullable<global::System.DateTime> LastUpdatedUtcField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string EntityName
		{
			get
			{
				return this.EntityNameField;
			}
			set
			{
				this.EntityNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> LastUpdatedUtc
		{
			get
			{
				return this.LastUpdatedUtcField;
			}
			set
			{
				this.LastUpdatedUtcField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="TaxiTime", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class TaxiTime : object
	{

		private string AirportField;

		private int TaxiInAvgField;

		private int TaxiOutAvgField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Airport
		{
			get
			{
				return this.AirportField;
			}
			set
			{
				this.AirportField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TaxiInAvg
		{
			get
			{
				return this.TaxiInAvgField;
			}
			set
			{
				this.TaxiInAvgField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TaxiOutAvg
		{
			get
			{
				return this.TaxiOutAvgField;
			}
			set
			{
				this.TaxiOutAvgField = value;
			}
		}
	}
}


namespace host
{
	public class AppDelegate
	{
		static void MainTest ()
		{
			var a = new DingusSyncData ();
			a.Aircraft = new AircraftDTO[] { new AircraftDTO () { } };
			a.AircraftTypes = new AircraftTypeDTO[] { new AircraftTypeDTO () };
			a.Airlines= new AirlineDTO[] { new AirlineDTO () };
			a.Airports= new AirportDTO[] { new AirportDTO() };
			a.Approaches= new ApproachDTO[] { new ApproachDTO() };
			a.ApproachesLegs= new ApproachesLegDTO[] { new ApproachesLegDTO() };
			a.Binaries= new BinaryCatalogDTO[] { new BinaryCatalogDTO() };
			a.Crews= new CrewDTO[] { new CrewDTO() };
			a.Days= new DayDTO[] { new DayDTO() };
			a.EmploymentEvents= new EmploymentEventDTO[] { new EmploymentEventDTO() };
			a.Events= new EventDTO[] { new EventDTO() };
			a.FlightDataInspection = new DataInspection ();
			a.GlobalSettings= new GlobalSettingDTO[] { new GlobalSettingDTO() };
			a.Hotels= new HotelDTO[] { new HotelDTO() };
			a.Legs= new LegDTO[] { new LegDTO() };
			a.Notes= new NoteDTO[] { new NoteDTO() };
			a.PayperiodEvents= new PayperiodEventDTO[] { new PayperiodEventDTO() };
			a.PayrollCategories= new PayrollCategoryDTO[] { new PayrollCategoryDTO() };
			a.Payrolls= new PayrollDTO[] { new PayrollDTO() };
			a.Performances= new PerformanceDTO[] { new PerformanceDTO() };
			a.Positions= new PositionDTO[] { new PositionDTO() };
			a.ReglatoryOperationTypes= new ReglatoryOperationTypeDTO[] { new ReglatoryOperationTypeDTO() };
			a.Trips= new TripDTO[] { new TripDTO() };
			a.UserSettings= new UserSettingDTO[] { new UserSettingDTO() };

			Console.WriteLine ("Size is: {0}", global::System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)));
			using (var ms = new MemoryStream ()) {
				DataContractSerializer serializer = new DataContractSerializer (typeof(DingusSyncData));
				serializer.WriteObject (ms, a);
				ms.Position = 0;
				var b = serializer.ReadObject (ms);
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DingusSyncData", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DingusSyncData : object
	{

		AircraftDTO[] AircraftField;

		AircraftTypeDTO[] AircraftTypesField;

		AirlineDTO[] AirlinesField;

		AirportDTO[] AirportsField;

		ApproachDTO[] ApproachesField;

		ApproachesLegDTO[] ApproachesLegsField;

		BinaryCatalogDTO[] BinariesField;

		CrewDTO[] CrewsField;

		DayDTO[] DaysField;

		EmploymentEventDTO[] EmploymentEventsField;

		EventDTO[] EventsField;

		DataInspection FlightDataInspectionField;

		GlobalSettingDTO[] GlobalSettingsField;

		HotelDTO[] HotelsField;

		LegDTO[] LegsField;

		NoteDTO[] NotesField;

		PayperiodEventDTO[] PayperiodEventsField;

		PayrollCategoryDTO[] PayrollCategoriesField;

		PayrollDTO[] PayrollsField;

		PerformanceDTO[] PerformancesField;

		PositionDTO[] PositionsField;

		ReglatoryOperationTypeDTO[] ReglatoryOperationTypesField;

		TripDTO[] TripsField;

		UserSettingDTO[] UserSettingsField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AircraftDTO[] Aircraft
		{
			get
			{
				return this.AircraftField;
			}
			set
			{
				this.AircraftField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AircraftTypeDTO[] AircraftTypes
		{
			get
			{
				return this.AircraftTypesField;
			}
			set
			{
				this.AircraftTypesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AirlineDTO[] Airlines
		{
			get
			{
				return this.AirlinesField;
			}
			set
			{
				this.AirlinesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public AirportDTO[] Airports
		{
			get
			{
				return this.AirportsField;
			}
			set
			{
				this.AirportsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ApproachDTO[] Approaches
		{
			get
			{
				return this.ApproachesField;
			}
			set
			{
				this.ApproachesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ApproachesLegDTO[] ApproachesLegs
		{
			get
			{
				return this.ApproachesLegsField;
			}
			set
			{
				this.ApproachesLegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public BinaryCatalogDTO[] Binaries
		{
			get
			{
				return this.BinariesField;
			}
			set
			{
				this.BinariesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public CrewDTO[] Crews
		{
			get
			{
				return this.CrewsField;
			}
			set
			{
				this.CrewsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DayDTO[] Days
		{
			get
			{
				return this.DaysField;
			}
			set
			{
				this.DaysField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EmploymentEventDTO[] EmploymentEvents
		{
			get
			{
				return this.EmploymentEventsField;
			}
			set
			{
				this.EmploymentEventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EventDTO[] Events
		{
			get
			{
				return this.EventsField;
			}
			set
			{
				this.EventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DataInspection FlightDataInspection
		{
			get
			{
				return this.FlightDataInspectionField;
			}
			set
			{
				this.FlightDataInspectionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public GlobalSettingDTO[] GlobalSettings
		{
			get
			{
				return this.GlobalSettingsField;
			}
			set
			{
				this.GlobalSettingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public HotelDTO[] Hotels
		{
			get
			{
				return this.HotelsField;
			}
			set
			{
				this.HotelsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public LegDTO[] Legs
		{
			get
			{
				return this.LegsField;
			}
			set
			{
				this.LegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public NoteDTO[] Notes
		{
			get
			{
				return this.NotesField;
			}
			set
			{
				this.NotesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayperiodEventDTO[] PayperiodEvents
		{
			get
			{
				return this.PayperiodEventsField;
			}
			set
			{
				this.PayperiodEventsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayrollCategoryDTO[] PayrollCategories
		{
			get
			{
				return this.PayrollCategoriesField;
			}
			set
			{
				this.PayrollCategoriesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PayrollDTO[] Payrolls
		{
			get
			{
				return this.PayrollsField;
			}
			set
			{
				this.PayrollsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PerformanceDTO[] Performances
		{
			get
			{
				return this.PerformancesField;
			}
			set
			{
				this.PerformancesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public PositionDTO[] Positions
		{
			get
			{
				return this.PositionsField;
			}
			set
			{
				this.PositionsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public ReglatoryOperationTypeDTO[] ReglatoryOperationTypes
		{
			get
			{
				return this.ReglatoryOperationTypesField;
			}
			set
			{
				this.ReglatoryOperationTypesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public TripDTO[] Trips
		{
			get
			{
				return this.TripsField;
			}
			set
			{
				this.TripsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public UserSettingDTO[] UserSettings
		{
			get
			{
				return this.UserSettingsField;
			}
			set
			{
				this.UserSettingsField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DataInspection", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DataInspection : object
	{

		private int DayCountField;

		private int LegCountField;

		private Nullable<global::System.DateTime> MaxTripSequenceEndField;

		private Nullable<global::System.DateTime> MinTripSequenceStartField;

		private int TripCountField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int DayCount
		{
			get
			{
				return this.DayCountField;
			}
			set
			{
				this.DayCountField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int LegCount
		{
			get
			{
				return this.LegCountField;
			}
			set
			{
				this.LegCountField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> MaxTripSequenceEnd
		{
			get
			{
				return this.MaxTripSequenceEndField;
			}
			set
			{
				this.MaxTripSequenceEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> MinTripSequenceStart
		{
			get
			{
				return this.MinTripSequenceStartField;
			}
			set
			{
				this.MinTripSequenceStartField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TripCount
		{
			get
			{
				return this.TripCountField;
			}
			set
			{
				this.TripCountField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AircraftDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AircraftDTO : object
	{

		private string aircraftIdField;

		private string aircraftTypeIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string currentAirlineIdField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<decimal> payrateField;

		private Nullable<bool> previewField;

		private string previousAirlineIdField;

		private string registrationField;

		private string shipNumberField;

		private Nullable<bool> syncedField;

		private string tailField;

		private Nullable<bool> usePayrateField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftId
		{
			get
			{
				return this.aircraftIdField;
			}
			set
			{
				this.aircraftIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftTypeId
		{
			get
			{
				return this.aircraftTypeIdField;
			}
			set
			{
				this.aircraftTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string currentAirlineId
		{
			get
			{
				return this.currentAirlineIdField;
			}
			set
			{
				this.currentAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> payrate
		{
			get
			{
				return this.payrateField;
			}
			set
			{
				this.payrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string previousAirlineId
		{
			get
			{
				return this.previousAirlineIdField;
			}
			set
			{
				this.previousAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string registration
		{
			get
			{
				return this.registrationField;
			}
			set
			{
				this.registrationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string shipNumber
		{
			get
			{
				return this.shipNumberField;
			}
			set
			{
				this.shipNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tail
		{
			get
			{
				return this.tailField;
			}
			set
			{
				this.tailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> usePayrate
		{
			get
			{
				return this.usePayrateField;
			}
			set
			{
				this.usePayrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AircraftTypeDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AircraftTypeDTO : object
	{

		private string aircraftTypeIdField;

		private string aselField;

		private string configField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string iconUrlField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private string transportField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftTypeId
		{
			get
			{
				return this.aircraftTypeIdField;
			}
			set
			{
				this.aircraftTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string asel
		{
			get
			{
				return this.aselField;
			}
			set
			{
				this.aselField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string config
		{
			get
			{
				return this.configField;
			}
			set
			{
				this.configField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string iconUrl
		{
			get
			{
				return this.iconUrlField;
			}
			set
			{
				this.iconUrlField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string transport
		{
			get
			{
				return this.transportField;
			}
			set
			{
				this.transportField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AirlineDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AirlineDTO : object
	{

		private string airlineIdField;

		private string airlineNameField;

		private string callSignField;

		private string countryField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string icaoField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string phoneField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineName
		{
			get
			{
				return this.airlineNameField;
			}
			set
			{
				this.airlineNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string callSign
		{
			get
			{
				return this.callSignField;
			}
			set
			{
				this.callSignField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string country
		{
			get
			{
				return this.countryField;
			}
			set
			{
				this.countryField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string icao
		{
			get
			{
				return this.icaoField;
			}
			set
			{
				this.icaoField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="AirportDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class AirportDTO : object
	{

		private string airlineNameField;

		private string airportIdField;

		private string airportNameField;

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<bool> dstField;

		private string emailField;

		private string faaField;

		private string iataField;

		private string icaoField;

		private string idField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<decimal> latitudeField;

		private string localityField;

		private string locationField;

		private Nullable<decimal> longitudeField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string olsentimezonenameField;

		private string phoneField;

		private string pictureField;

		private Nullable<bool> previewField;

		private Nullable<int> privacyLevelField;

		private string regionField;

		private Nullable<bool> syncedField;

		private int userIdField;

		private Nullable<decimal> utcoffsetField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineName
		{
			get
			{
				return this.airlineNameField;
			}
			set
			{
				this.airlineNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airportId
		{
			get
			{
				return this.airportIdField;
			}
			set
			{
				this.airportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airportName
		{
			get
			{
				return this.airportNameField;
			}
			set
			{
				this.airportNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> dst
		{
			get
			{
				return this.dstField;
			}
			set
			{
				this.dstField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string faa
		{
			get
			{
				return this.faaField;
			}
			set
			{
				this.faaField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string iata
		{
			get
			{
				return this.iataField;
			}
			set
			{
				this.iataField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string icao
		{
			get
			{
				return this.icaoField;
			}
			set
			{
				this.icaoField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string id
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> latitude
		{
			get
			{
				return this.latitudeField;
			}
			set
			{
				this.latitudeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string locality
		{
			get
			{
				return this.localityField;
			}
			set
			{
				this.localityField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> longitude
		{
			get
			{
				return this.longitudeField;
			}
			set
			{
				this.longitudeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string olsentimezonename
		{
			get
			{
				return this.olsentimezonenameField;
			}
			set
			{
				this.olsentimezonenameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string region
		{
			get
			{
				return this.regionField;
			}
			set
			{
				this.regionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> utcoffset
		{
			get
			{
				return this.utcoffsetField;
			}
			set
			{
				this.utcoffsetField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ApproachDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ApproachDTO : object
	{

		private string approachIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private string typeField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ApproachesLegDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ApproachesLegDTO : object
	{

		private string approachIdField;

		private string approachesLegsIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string legIdField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachesLegsId
		{
			get
			{
				return this.approachesLegsIdField;
			}
			set
			{
				this.approachesLegsIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string legId
		{
			get
			{
				return this.legIdField;
			}
			set
			{
				this.legIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="BinaryCatalogDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class BinaryCatalogDTO : object
	{

		private global::System.Guid RowGuidField;

		private Nullable<int> areaIdField;

		private string binaryCatalogIdField;

		private Nullable<int> contentLengthField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string filenameField;

		private string folderIdField;

		private Nullable<bool> isSecuredField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<global::System.DateTime> lastWriteTimeUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private string titleField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.Guid RowGuid
		{
			get
			{
				return this.RowGuidField;
			}
			set
			{
				this.RowGuidField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> areaId
		{
			get
			{
				return this.areaIdField;
			}
			set
			{
				this.areaIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string binaryCatalogId
		{
			get
			{
				return this.binaryCatalogIdField;
			}
			set
			{
				this.binaryCatalogIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> contentLength
		{
			get
			{
				return this.contentLengthField;
			}
			set
			{
				this.contentLengthField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string filename
		{
			get
			{
				return this.filenameField;
			}
			set
			{
				this.filenameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string folderId
		{
			get
			{
				return this.folderIdField;
			}
			set
			{
				this.folderIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> isSecured
		{
			get
			{
				return this.isSecuredField;
			}
			set
			{
				this.isSecuredField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastWriteTimeUtc
		{
			get
			{
				return this.lastWriteTimeUtcField;
			}
			set
			{
				this.lastWriteTimeUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string title
		{
			get
			{
				return this.titleField;
			}
			set
			{
				this.titleField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="CrewDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class CrewDTO : object
	{

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string crewIdField;

		private string crewNameField;

		private string currentAirlineIdField;

		private Nullable<bool> deletedField;

		private string emailField;

		private string facebookField;

		private string idField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string locationField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string phoneField;

		private string pictureField;

		private string positionField;

		private Nullable<bool> previewField;

		private string previousAirlineIdField;

		private Nullable<int> privacyLevelField;

		private Nullable<bool> syncedField;

		private string twitterField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string crewId
		{
			get
			{
				return this.crewIdField;
			}
			set
			{
				this.crewIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string crewName
		{
			get
			{
				return this.crewNameField;
			}
			set
			{
				this.crewNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string currentAirlineId
		{
			get
			{
				return this.currentAirlineIdField;
			}
			set
			{
				this.currentAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string facebook
		{
			get
			{
				return this.facebookField;
			}
			set
			{
				this.facebookField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string id
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string position
		{
			get
			{
				return this.positionField;
			}
			set
			{
				this.positionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string previousAirlineId
		{
			get
			{
				return this.previousAirlineIdField;
			}
			set
			{
				this.previousAirlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string twitter
		{
			get
			{
				return this.twitterField;
			}
			set
			{
				this.twitterField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DayDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DayDTO : object
	{

		private Nullable<int> blockField;

		private string calendarIdentifierField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<int> creditField;

		private string dayIdField;

		private Nullable<bool> deletedField;

		private Nullable<decimal> dutyField;

		private Nullable<global::System.DateTime> dutyOffField;

		private Nullable<global::System.DateTime> dutyOnField;

		private Nullable<int> fDPField;

		private Nullable<global::System.DateTime> fDPEndTimeField;

		private Nullable<decimal> flightTimeField;

		private Nullable<int> grossPayField;

		private string hotelIdField;

		private Nullable<decimal> instrumentField;

		private Nullable<int> landingsField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<decimal> nightField;

		private Nullable<int> nightLandingsField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> overrideDutyOffField;

		private Nullable<bool> overrideDutyOnField;

		private Nullable<bool> previewField;

		private Nullable<int> rdpField;

		private Nullable<global::System.DateTime> rdpBeginField;

		private Nullable<global::System.DateTime> rdpEndField;

		private Nullable<int> scheduleBlockField;

		private Nullable<int> splitDutyField;

		private Nullable<global::System.DateTime> splitDutyBeginField;

		private Nullable<global::System.DateTime> splitDutyEndField;

		private Nullable<bool> syncedField;

		private string tripIdField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> block
		{
			get
			{
				return this.blockField;
			}
			set
			{
				this.blockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> credit
		{
			get
			{
				return this.creditField;
			}
			set
			{
				this.creditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dayId
		{
			get
			{
				return this.dayIdField;
			}
			set
			{
				this.dayIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> duty
		{
			get
			{
				return this.dutyField;
			}
			set
			{
				this.dutyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dutyOff
		{
			get
			{
				return this.dutyOffField;
			}
			set
			{
				this.dutyOffField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dutyOn
		{
			get
			{
				return this.dutyOnField;
			}
			set
			{
				this.dutyOnField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> fDP
		{
			get
			{
				return this.fDPField;
			}
			set
			{
				this.fDPField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> fDPEndTime
		{
			get
			{
				return this.fDPEndTimeField;
			}
			set
			{
				this.fDPEndTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> flightTime
		{
			get
			{
				return this.flightTimeField;
			}
			set
			{
				this.flightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> grossPay
		{
			get
			{
				return this.grossPayField;
			}
			set
			{
				this.grossPayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelId
		{
			get
			{
				return this.hotelIdField;
			}
			set
			{
				this.hotelIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> instrument
		{
			get
			{
				return this.instrumentField;
			}
			set
			{
				this.instrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> landings
		{
			get
			{
				return this.landingsField;
			}
			set
			{
				this.landingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> night
		{
			get
			{
				return this.nightField;
			}
			set
			{
				this.nightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> nightLandings
		{
			get
			{
				return this.nightLandingsField;
			}
			set
			{
				this.nightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overrideDutyOff
		{
			get
			{
				return this.overrideDutyOffField;
			}
			set
			{
				this.overrideDutyOffField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overrideDutyOn
		{
			get
			{
				return this.overrideDutyOnField;
			}
			set
			{
				this.overrideDutyOnField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rdp
		{
			get
			{
				return this.rdpField;
			}
			set
			{
				this.rdpField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> rdpBegin
		{
			get
			{
				return this.rdpBeginField;
			}
			set
			{
				this.rdpBeginField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> rdpEnd
		{
			get
			{
				return this.rdpEndField;
			}
			set
			{
				this.rdpEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> scheduleBlock
		{
			get
			{
				return this.scheduleBlockField;
			}
			set
			{
				this.scheduleBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> splitDuty
		{
			get
			{
				return this.splitDutyField;
			}
			set
			{
				this.splitDutyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> splitDutyBegin
		{
			get
			{
				return this.splitDutyBeginField;
			}
			set
			{
				this.splitDutyBeginField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> splitDutyEnd
		{
			get
			{
				return this.splitDutyEndField;
			}
			set
			{
				this.splitDutyEndField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripId
		{
			get
			{
				return this.tripIdField;
			}
			set
			{
				this.tripIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EmploymentEventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EmploymentEventDTO : object
	{

		private string airlineIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string employmentEventIdField;

		private Nullable<global::System.DateTime> firstDateField;

		private Nullable<global::System.DateTime> lastDateField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string employmentEventId
		{
			get
			{
				return this.employmentEventIdField;
			}
			set
			{
				this.employmentEventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> firstDate
		{
			get
			{
				return this.firstDateField;
			}
			set
			{
				this.firstDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastDate
		{
			get
			{
				return this.lastDateField;
			}
			set
			{
				this.lastDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EventDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<global::System.DateTime> dateRangeField;

		private Nullable<bool> deletedField;

		private string eventIdField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<int> previewField;

		private Nullable<bool> syncedField;

		private string urlField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> dateRange
		{
			get
			{
				return this.dateRangeField;
			}
			set
			{
				this.dateRangeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string eventId
		{
			get
			{
				return this.eventIdField;
			}
			set
			{
				this.eventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string url
		{
			get
			{
				return this.urlField;
			}
			set
			{
				this.urlField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="GlobalSettingDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class GlobalSettingDTO : object
	{

		private string DescriptionField;

		private global::System.Guid GlobalSettingIdField;

		private global::System.DateTime LastUpdatedUtcField;

		private string SettingKeyField;

		private string SettingValueField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Description
		{
			get
			{
				return this.DescriptionField;
			}
			set
			{
				this.DescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.Guid GlobalSettingId
		{
			get
			{
				return this.GlobalSettingIdField;
			}
			set
			{
				this.GlobalSettingIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime LastUpdatedUtc
		{
			get
			{
				return this.LastUpdatedUtcField;
			}
			set
			{
				this.LastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string SettingKey
		{
			get
			{
				return this.SettingKeyField;
			}
			set
			{
				this.SettingKeyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string SettingValue
		{
			get
			{
				return this.SettingValueField;
			}
			set
			{
				this.SettingValueField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="HotelDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class HotelDTO : object
	{

		private Nullable<int> communicationLevelField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string emailField;

		private string hotelIdField;

		private string hotelNameField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string locationField;

		private Nullable<int> modelVersionField;

		private string nameField;

		private string notesField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string numberField;

		private string phoneField;

		private string pictureField;

		private Nullable<bool> previewField;

		private Nullable<int> privacyLevelField;

		private Nullable<int> ratingsField;

		private string sharedNotesField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> communicationLevel
		{
			get
			{
				return this.communicationLevelField;
			}
			set
			{
				this.communicationLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string email
		{
			get
			{
				return this.emailField;
			}
			set
			{
				this.emailField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelId
		{
			get
			{
				return this.hotelIdField;
			}
			set
			{
				this.hotelIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string hotelName
		{
			get
			{
				return this.hotelNameField;
			}
			set
			{
				this.hotelNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string location
		{
			get
			{
				return this.locationField;
			}
			set
			{
				this.locationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notes
		{
			get
			{
				return this.notesField;
			}
			set
			{
				this.notesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string number
		{
			get
			{
				return this.numberField;
			}
			set
			{
				this.numberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string phone
		{
			get
			{
				return this.phoneField;
			}
			set
			{
				this.phoneField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string picture
		{
			get
			{
				return this.pictureField;
			}
			set
			{
				this.pictureField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> privacyLevel
		{
			get
			{
				return this.privacyLevelField;
			}
			set
			{
				this.privacyLevelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> ratings
		{
			get
			{
				return this.ratingsField;
			}
			set
			{
				this.ratingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string sharedNotes
		{
			get
			{
				return this.sharedNotesField;
			}
			set
			{
				this.sharedNotesField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="LegDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class LegDTO : object
	{

		private Nullable<int> actualBlockField;

		private string aircraftIdField;

		private string approachIdField;

		private string cabinCrewAIdField;

		private string cabinCrewBIdField;

		private string calendarIdentifierField;

		private string captainIdField;

		private Nullable<bool> completedField;

		private Nullable<global::System.DateTime> createdUtcField;

		private string dayIdField;

		private Nullable<bool> deletedField;

		private string departureAirportIdField;

		private string deptField;

		private string deptGateField;

		private string destField;

		private string destGateField;

		private string destinationAirportIdField;

		private Nullable<global::System.DateTime> etaField;

		private string firstOfficerIdField;

		private string flightNumberField;

		private Nullable<int> flightTimeField;

		private Nullable<global::System.DateTime> inOOOIField;

		private Nullable<decimal> instrumentField;

		private Nullable<int> landingsField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private string legIdField;

		private Nullable<int> modelVersionField;

		private Nullable<decimal> nightField;

		private Nullable<int> nightLandingsField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<global::System.DateTime> offOOOIField;

		private Nullable<global::System.DateTime> onOOOIField;

		private string operationTypeIdField;

		private string otherCrewAIdField;

		private string otherCrewBIdField;

		private Nullable<global::System.DateTime> outOOOIField;

		private string payIdField;

		private string positionIdField;

		private Nullable<bool> previewField;

		private string registrationField;

		private string remarksField;

		private Nullable<int> scheduledBlockField;

		private Nullable<global::System.DateTime> scheduledInField;

		private Nullable<global::System.DateTime> scheduledOutField;

		private Nullable<int> sequenceField;

		private Nullable<bool> syncedField;

		private Nullable<int> taxiTimeInField;

		private Nullable<int> taxiTimeOutField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actualBlock
		{
			get
			{
				return this.actualBlockField;
			}
			set
			{
				this.actualBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string aircraftId
		{
			get
			{
				return this.aircraftIdField;
			}
			set
			{
				this.aircraftIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string approachId
		{
			get
			{
				return this.approachIdField;
			}
			set
			{
				this.approachIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string cabinCrewAId
		{
			get
			{
				return this.cabinCrewAIdField;
			}
			set
			{
				this.cabinCrewAIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string cabinCrewBId
		{
			get
			{
				return this.cabinCrewBIdField;
			}
			set
			{
				this.cabinCrewBIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string captainId
		{
			get
			{
				return this.captainIdField;
			}
			set
			{
				this.captainIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> completed
		{
			get
			{
				return this.completedField;
			}
			set
			{
				this.completedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dayId
		{
			get
			{
				return this.dayIdField;
			}
			set
			{
				this.dayIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string departureAirportId
		{
			get
			{
				return this.departureAirportIdField;
			}
			set
			{
				this.departureAirportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dept
		{
			get
			{
				return this.deptField;
			}
			set
			{
				this.deptField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string deptGate
		{
			get
			{
				return this.deptGateField;
			}
			set
			{
				this.deptGateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string dest
		{
			get
			{
				return this.destField;
			}
			set
			{
				this.destField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string destGate
		{
			get
			{
				return this.destGateField;
			}
			set
			{
				this.destGateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string destinationAirportId
		{
			get
			{
				return this.destinationAirportIdField;
			}
			set
			{
				this.destinationAirportIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> eta
		{
			get
			{
				return this.etaField;
			}
			set
			{
				this.etaField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string firstOfficerId
		{
			get
			{
				return this.firstOfficerIdField;
			}
			set
			{
				this.firstOfficerIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string flightNumber
		{
			get
			{
				return this.flightNumberField;
			}
			set
			{
				this.flightNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> flightTime
		{
			get
			{
				return this.flightTimeField;
			}
			set
			{
				this.flightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> inOOOI
		{
			get
			{
				return this.inOOOIField;
			}
			set
			{
				this.inOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> instrument
		{
			get
			{
				return this.instrumentField;
			}
			set
			{
				this.instrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> landings
		{
			get
			{
				return this.landingsField;
			}
			set
			{
				this.landingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string legId
		{
			get
			{
				return this.legIdField;
			}
			set
			{
				this.legIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> night
		{
			get
			{
				return this.nightField;
			}
			set
			{
				this.nightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> nightLandings
		{
			get
			{
				return this.nightLandingsField;
			}
			set
			{
				this.nightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> offOOOI
		{
			get
			{
				return this.offOOOIField;
			}
			set
			{
				this.offOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> onOOOI
		{
			get
			{
				return this.onOOOIField;
			}
			set
			{
				this.onOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationTypeId
		{
			get
			{
				return this.operationTypeIdField;
			}
			set
			{
				this.operationTypeIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string otherCrewAId
		{
			get
			{
				return this.otherCrewAIdField;
			}
			set
			{
				this.otherCrewAIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string otherCrewBId
		{
			get
			{
				return this.otherCrewBIdField;
			}
			set
			{
				this.otherCrewBIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> outOOOI
		{
			get
			{
				return this.outOOOIField;
			}
			set
			{
				this.outOOOIField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payId
		{
			get
			{
				return this.payIdField;
			}
			set
			{
				this.payIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string positionId
		{
			get
			{
				return this.positionIdField;
			}
			set
			{
				this.positionIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string registration
		{
			get
			{
				return this.registrationField;
			}
			set
			{
				this.registrationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string remarks
		{
			get
			{
				return this.remarksField;
			}
			set
			{
				this.remarksField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> scheduledBlock
		{
			get
			{
				return this.scheduledBlockField;
			}
			set
			{
				this.scheduledBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> scheduledIn
		{
			get
			{
				return this.scheduledInField;
			}
			set
			{
				this.scheduledInField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> scheduledOut
		{
			get
			{
				return this.scheduledOutField;
			}
			set
			{
				this.scheduledOutField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> sequence
		{
			get
			{
				return this.sequenceField;
			}
			set
			{
				this.sequenceField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> taxiTimeIn
		{
			get
			{
				return this.taxiTimeInField;
			}
			set
			{
				this.taxiTimeInField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> taxiTimeOut
		{
			get
			{
				return this.taxiTimeOutField;
			}
			set
			{
				this.taxiTimeOutField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="NoteDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class NoteDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string note1Field;

		private string noteIdField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string note1
		{
			get
			{
				return this.note1Field;
			}
			set
			{
				this.note1Field = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string noteId
		{
			get
			{
				return this.noteIdField;
			}
			set
			{
				this.noteIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayperiodEventDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayperiodEventDTO : object
	{

		private string airlineIdField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> firstDateField;

		private Nullable<global::System.DateTime> lastDateField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string payperiodEventIdField;

		private string periodDescriptionField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string airlineId
		{
			get
			{
				return this.airlineIdField;
			}
			set
			{
				this.airlineIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> firstDate
		{
			get
			{
				return this.firstDateField;
			}
			set
			{
				this.firstDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastDate
		{
			get
			{
				return this.lastDateField;
			}
			set
			{
				this.lastDateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payperiodEventId
		{
			get
			{
				return this.payperiodEventIdField;
			}
			set
			{
				this.payperiodEventIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string periodDescription
		{
			get
			{
				return this.periodDescriptionField;
			}
			set
			{
				this.periodDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayrollCategoryDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayrollCategoryDTO : object
	{

		private Nullable<bool> aboveGuaranteeField;

		private Nullable<bool> applyRigField;

		private Nullable<bool> applyToFlightTimeField;

		private Nullable<bool> applyToLegalityField;

		private Nullable<bool> applyToPayField;

		private Nullable<bool> copyLegField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> minimumCreditField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> overridePayField;

		private Nullable<decimal> payrateField;

		private string payrollCategoriesIdField;

		private string plainDescriptionField;

		private Nullable<bool> previewField;

		private Nullable<int> rigAField;

		private Nullable<int> rigBField;

		private Nullable<bool> selectableField;

		private Nullable<bool> setAllLegsField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> aboveGuarantee
		{
			get
			{
				return this.aboveGuaranteeField;
			}
			set
			{
				this.aboveGuaranteeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyRig
		{
			get
			{
				return this.applyRigField;
			}
			set
			{
				this.applyRigField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToFlightTime
		{
			get
			{
				return this.applyToFlightTimeField;
			}
			set
			{
				this.applyToFlightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToLegality
		{
			get
			{
				return this.applyToLegalityField;
			}
			set
			{
				this.applyToLegalityField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> applyToPay
		{
			get
			{
				return this.applyToPayField;
			}
			set
			{
				this.applyToPayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> copyLeg
		{
			get
			{
				return this.copyLegField;
			}
			set
			{
				this.copyLegField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> minimumCredit
		{
			get
			{
				return this.minimumCreditField;
			}
			set
			{
				this.minimumCreditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> overridePay
		{
			get
			{
				return this.overridePayField;
			}
			set
			{
				this.overridePayField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> payrate
		{
			get
			{
				return this.payrateField;
			}
			set
			{
				this.payrateField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollCategoriesId
		{
			get
			{
				return this.payrollCategoriesIdField;
			}
			set
			{
				this.payrollCategoriesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string plainDescription
		{
			get
			{
				return this.plainDescriptionField;
			}
			set
			{
				this.plainDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rigA
		{
			get
			{
				return this.rigAField;
			}
			set
			{
				this.rigAField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> rigB
		{
			get
			{
				return this.rigBField;
			}
			set
			{
				this.rigBField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> setAllLegs
		{
			get
			{
				return this.setAllLegsField;
			}
			set
			{
				this.setAllLegsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PayrollDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PayrollDTO : object
	{

		private Nullable<int> actualField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<int> creditField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string payrollCategoriesIdField;

		private string payrollIdField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actual
		{
			get
			{
				return this.actualField;
			}
			set
			{
				this.actualField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> credit
		{
			get
			{
				return this.creditField;
			}
			set
			{
				this.creditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollCategoriesId
		{
			get
			{
				return this.payrollCategoriesIdField;
			}
			set
			{
				this.payrollCategoriesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string payrollId
		{
			get
			{
				return this.payrollIdField;
			}
			set
			{
				this.payrollIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PerformanceDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PerformanceDTO : object
	{

		private Nullable<int> actualField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<int> deviatedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private string performanceIdField;

		private Nullable<int> plannedFuelField;

		private Nullable<bool> previewField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> actual
		{
			get
			{
				return this.actualField;
			}
			set
			{
				this.actualField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> deviated
		{
			get
			{
				return this.deviatedField;
			}
			set
			{
				this.deviatedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string performanceId
		{
			get
			{
				return this.performanceIdField;
			}
			set
			{
				this.performanceIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> plannedFuel
		{
			get
			{
				return this.plannedFuelField;
			}
			set
			{
				this.plannedFuelField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="PositionDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class PositionDTO : object
	{

		private Nullable<bool> autoNightLandingField;

		private Nullable<bool> checkAirmanField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> creditLandingField;

		private Nullable<bool> deletedField;

		private Nullable<bool> ioeField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> pilotFlyingField;

		private Nullable<bool> pilotInCommandField;

		private string position1Field;

		private string positionIdField;

		private Nullable<bool> previewField;

		private Nullable<bool> selectableField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> autoNightLanding
		{
			get
			{
				return this.autoNightLandingField;
			}
			set
			{
				this.autoNightLandingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> checkAirman
		{
			get
			{
				return this.checkAirmanField;
			}
			set
			{
				this.checkAirmanField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> creditLanding
		{
			get
			{
				return this.creditLandingField;
			}
			set
			{
				this.creditLandingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> ioe
		{
			get
			{
				return this.ioeField;
			}
			set
			{
				this.ioeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> pilotFlying
		{
			get
			{
				return this.pilotFlyingField;
			}
			set
			{
				this.pilotFlyingField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> pilotInCommand
		{
			get
			{
				return this.pilotInCommandField;
			}
			set
			{
				this.pilotInCommandField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string position1
		{
			get
			{
				return this.position1Field;
			}
			set
			{
				this.position1Field = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string positionId
		{
			get
			{
				return this.positionIdField;
			}
			set
			{
				this.positionIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> selectable
		{
			get
			{
				return this.selectableField;
			}
			set
			{
				this.selectableField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="ReglatoryOperationTypeDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class ReglatoryOperationTypeDTO : object
	{

		private Nullable<bool> activeField;

		private Nullable<bool> canMixOperationsField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string operationAbvreviationField;

		private string operationDescriptionField;

		private Nullable<bool> previewField;

		private string reglatoryOperationTypesIdField;

		private string schemaURLField;

		private Nullable<bool> syncedField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> active
		{
			get
			{
				return this.activeField;
			}
			set
			{
				this.activeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> canMixOperations
		{
			get
			{
				return this.canMixOperationsField;
			}
			set
			{
				this.canMixOperationsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationAbvreviation
		{
			get
			{
				return this.operationAbvreviationField;
			}
			set
			{
				this.operationAbvreviationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string operationDescription
		{
			get
			{
				return this.operationDescriptionField;
			}
			set
			{
				this.operationDescriptionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string reglatoryOperationTypesId
		{
			get
			{
				return this.reglatoryOperationTypesIdField;
			}
			set
			{
				this.reglatoryOperationTypesIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string schemaURL
		{
			get
			{
				return this.schemaURLField;
			}
			set
			{
				this.schemaURLField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="TripDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class TripDTO : object
	{

		private bool activeField;

		private string calendarIdentifierField;

		private Nullable<bool> completedField;

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private Nullable<global::System.DateTime> seqEndTimeField;

		private Nullable<global::System.DateTime> seqStartTimeField;

		private Nullable<bool> syncedField;

		private Nullable<int> tafbField;

		private Nullable<int> totalBlockField;

		private Nullable<int> totalCreditField;

		private Nullable<decimal> totalFlightTimeField;

		private Nullable<decimal> totalInstrumentField;

		private Nullable<int> totalLandingsField;

		private Nullable<decimal> totalNightField;

		private Nullable<int> totalNightLandingsField;

		private Nullable<decimal> totalPayFField;

		private string tripIdField;

		private string tripNumberField;

		private int userIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool active
		{
			get
			{
				return this.activeField;
			}
			set
			{
				this.activeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string calendarIdentifier
		{
			get
			{
				return this.calendarIdentifierField;
			}
			set
			{
				this.calendarIdentifierField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> completed
		{
			get
			{
				return this.completedField;
			}
			set
			{
				this.completedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> seqEndTime
		{
			get
			{
				return this.seqEndTimeField;
			}
			set
			{
				this.seqEndTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> seqStartTime
		{
			get
			{
				return this.seqStartTimeField;
			}
			set
			{
				this.seqStartTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> tafb
		{
			get
			{
				return this.tafbField;
			}
			set
			{
				this.tafbField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalBlock
		{
			get
			{
				return this.totalBlockField;
			}
			set
			{
				this.totalBlockField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalCredit
		{
			get
			{
				return this.totalCreditField;
			}
			set
			{
				this.totalCreditField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalFlightTime
		{
			get
			{
				return this.totalFlightTimeField;
			}
			set
			{
				this.totalFlightTimeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalInstrument
		{
			get
			{
				return this.totalInstrumentField;
			}
			set
			{
				this.totalInstrumentField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalLandings
		{
			get
			{
				return this.totalLandingsField;
			}
			set
			{
				this.totalLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalNight
		{
			get
			{
				return this.totalNightField;
			}
			set
			{
				this.totalNightField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> totalNightLandings
		{
			get
			{
				return this.totalNightLandingsField;
			}
			set
			{
				this.totalNightLandingsField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<decimal> totalPayF
		{
			get
			{
				return this.totalPayFField;
			}
			set
			{
				this.totalPayFField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripId
		{
			get
			{
				return this.tripIdField;
			}
			set
			{
				this.tripIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string tripNumber
		{
			get
			{
				return this.tripNumberField;
			}
			set
			{
				this.tripNumberField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="UserSettingDTO", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class UserSettingDTO : object
	{

		private Nullable<global::System.DateTime> createdUtcField;

		private Nullable<bool> deletedField;

		private string keyField;

		private Nullable<global::System.DateTime> lastUpdatedUtcField;

		private Nullable<int> modelVersionField;

		private string notificationMessageField;

		private Nullable<int> notificationTypeField;

		private Nullable<bool> previewField;

		private string stringValueField;

		private Nullable<bool> syncedField;

		private int userIdField;

		private string userSettingIdField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> createdUtc
		{
			get
			{
				return this.createdUtcField;
			}
			set
			{
				this.createdUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> deleted
		{
			get
			{
				return this.deletedField;
			}
			set
			{
				this.deletedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string key
		{
			get
			{
				return this.keyField;
			}
			set
			{
				this.keyField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> lastUpdatedUtc
		{
			get
			{
				return this.lastUpdatedUtcField;
			}
			set
			{
				this.lastUpdatedUtcField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> modelVersion
		{
			get
			{
				return this.modelVersionField;
			}
			set
			{
				this.modelVersionField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string notificationMessage
		{
			get
			{
				return this.notificationMessageField;
			}
			set
			{
				this.notificationMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<int> notificationType
		{
			get
			{
				return this.notificationTypeField;
			}
			set
			{
				this.notificationTypeField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> preview
		{
			get
			{
				return this.previewField;
			}
			set
			{
				this.previewField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string stringValue
		{
			get
			{
				return this.stringValueField;
			}
			set
			{
				this.stringValueField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<bool> synced
		{
			get
			{
				return this.syncedField;
			}
			set
			{
				this.syncedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int userId
		{
			get
			{
				return this.userIdField;
			}
			set
			{
				this.userIdField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string userSettingId
		{
			get
			{
				return this.userSettingIdField;
			}
			set
			{
				this.userSettingIdField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="DingusSyncResponse", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class DingusSyncResponse : object
	{

		DingusSyncData CloudDataField;

		private string StatusField;

		private bool SuccessField;

		private global::System.DateTime SyncDateLineField;

		private long SyncDurationField;

		private global::System.DateTime SyncEndedField;

		private global::System.DateTime SyncStartedField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public DingusSyncData CloudData
		{
			get
			{
				return this.CloudDataField;
			}
			set
			{
				this.CloudDataField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Status
		{
			get
			{
				return this.StatusField;
			}
			set
			{
				this.StatusField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool Success
		{
			get
			{
				return this.SuccessField;
			}
			set
			{
				this.SuccessField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncDateLine
		{
			get
			{
				return this.SyncDateLineField;
			}
			set
			{
				this.SyncDateLineField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public long SyncDuration
		{
			get
			{
				return this.SyncDurationField;
			}
			set
			{
				this.SyncDurationField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncEnded
		{
			get
			{
				return this.SyncEndedField;
			}
			set
			{
				this.SyncEndedField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public global::System.DateTime SyncStarted
		{
			get
			{
				return this.SyncStartedField;
			}
			set
			{
				this.SyncStartedField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="BinaryTransferResponse", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class BinaryTransferResponse : object
	{

		private string ErrorMessageField;

		private bool SuccessField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string ErrorMessage
		{
			get
			{
				return this.ErrorMessageField;
			}
			set
			{
				this.ErrorMessageField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public bool Success
		{
			get
			{
				return this.SuccessField;
			}
			set
			{
				this.SuccessField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="SyncStatus", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class SyncStatus : object
	{

		EntitySyncState[] SyncStateField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public EntitySyncState[] SyncState
		{
			get
			{
				return this.SyncStateField;
			}
			set
			{
				this.SyncStateField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="EntitySyncState", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class EntitySyncState : object
	{

		private string EntityNameField;

		private Nullable<global::System.DateTime> LastUpdatedUtcField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string EntityName
		{
			get
			{
				return this.EntityNameField;
			}
			set
			{
				this.EntityNameField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public Nullable<global::System.DateTime> LastUpdatedUtc
		{
			get
			{
				return this.LastUpdatedUtcField;
			}
			set
			{
				this.LastUpdatedUtcField = value;
			}
		}
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("global::System.Runtime.Serialization", "4.0.0.0")]
	[global::System.Runtime.Serialization.DataContractAttribute(Name="TaxiTime", Namespace="http://schemas.datacontract.org/2004/07/Dingus.Data.DataContracts")]
	public partial class TaxiTime : object
	{

		private string AirportField;

		private int TaxiInAvgField;

		private int TaxiOutAvgField;

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public string Airport
		{
			get
			{
				return this.AirportField;
			}
			set
			{
				this.AirportField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TaxiInAvg
		{
			get
			{
				return this.TaxiInAvgField;
			}
			set
			{
				this.TaxiInAvgField = value;
			}
		}

		[global::System.Runtime.Serialization.DataMemberAttribute()]
		public int TaxiOutAvg
		{
			get
			{
				return this.TaxiOutAvgField;
			}
			set
			{
				this.TaxiOutAvgField = value;
			}
		}
	}
}



