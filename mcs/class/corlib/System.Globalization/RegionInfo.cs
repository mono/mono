using System.Globalization;

namespace System.Globalization {

	public class RegionInfo {
		int NLS_id;

		public RegionInfo (int culture) {

			if (CultureInfo.IsIDNeutralCulture (culture))
				throw new ArgumentException ("Culture ID " + culture
							 + " (0x" + culture.ToString ("X4")
							 + ") is a neutral culture. A region can not be created from it.");

			switch (culture) {
			case 0x0401: // ar-SA Arabic (Saudi Arabia)
			case 0x0801: // ar-IQ Arabic (Iraq)
			case 0x0C01: // ar-EG Arabic (Egypt)
			case 0x1001: // ar-LY Arabic (Libya)
			case 0x1401: // ar-DZ Arabic (Algeria)
			case 0x1801: // ar-MA Arabic (Morocco)
			case 0x1C01: // ar-TN Arabic (Tunisia)
			case 0x2001: // ar-OM Arabic (Oman)
			case 0x2401: // ar-YE Arabic (Yemen)
			case 0x2801: // ar-SY Arabic (Syria)
			case 0x2C01: // ar-JO Arabic (Jordan)
			case 0x3001: // ar-LB Arabic (Lebanon)
			case 0x3401: // ar-KW Arabic (Kuwait)
			case 0x3801: // ar-AE Arabic (U.A.E.)
			case 0x3C01: // ar-BH Arabic (Bahrain)
			case 0x4001: // ar-QA Arabic (Qatar)
			case 0x0402: // bg-BG Bulgarian (Bulgaria)
			case 0x0403: // ca-ES Catalan (Spain)
			case 0x0004: // zh-CHS Chinese (Simplified)
			case 0x0404: // zh-TW Chinese (Taiwan)
			case 0x0804: // zh-CN Chinese (People's Republic of China)
			case 0x0C04: // zh-HK Chinese (Hong Kong S.A.R.)
			case 0x1004: // zh-SG Chinese (Singapore)
			case 0x1404: // zh-MO Chinese (Macau S.A.R.)
			case 0x7C04: // zh-CHT Chinese (Traditional)
			case 0x0405: // cs-CZ Czech (Czech Republic)
			case 0x0406: // da-DK Danish (Denmark)
			case 0x0407: // de-DE German (Germany)
			case 0x0807: // de-CH German (Switzerland)
			case 0x0C07: // de-AT German (Austria)
			case 0x1007: // de-LU German (Luxembourg)
			case 0x1407: // de-LI German (Liechtenstein)
			case 0x0408: // el-GR Greek (Greece)
			case 0x0409: // en-US English (United States)
			case 0x0809: // en-GB English (United Kingdom)
			case 0x0C09: // en-AU English (Australia)
			case 0x1009: // en-CA English (Canada)
			case 0x1409: // en-NZ English (New Zealand)
			case 0x1809: // en-IE English (Ireland)
			case 0x1C09: // en-ZA English (South Africa)
			case 0x2009: // en-JM English (Jamaica)
			case 0x2409: // en-CB English (Caribbean)
			case 0x2809: // en-BZ English (Belize)
			case 0x2C09: // en-TT English (Trinidad and Tobago)
			case 0x3009: // en-ZW English (Zimbabwe)
			case 0x3409: // en-PH English (Republic of the Philippines)
			case 0x080A: // es-MX Spanish (Mexico)
			case 0x0C0A: // es-ES Spanish (Spain)
			case 0x100A: // es-GT Spanish (Guatemala)
			case 0x140A: // es-CR Spanish (Costa Rica)
			case 0x180A: // es-PA Spanish (Panama)
			case 0x1C0A: // es-DO Spanish (Dominican Republic)
			case 0x200A: // es-VE Spanish (Venezuela)
			case 0x240A: // es-CO Spanish (Colombia)
			case 0x280A: // es-PE Spanish (Peru)
			case 0x2C0A: // es-AR Spanish (Argentina)
			case 0x300A: // es-EC Spanish (Ecuador)
			case 0x340A: // es-CL Spanish (Chile)
			case 0x380A: // es-UY Spanish (Uruguay)
			case 0x3C0A: // es-PY Spanish (Paraguay)
			case 0x400A: // es-BO Spanish (Bolivia)
			case 0x440A: // es-SV Spanish (El Salvador)
			case 0x480A: // es-HN Spanish (Honduras)
			case 0x4C0A: // es-NI Spanish (Nicaragua)
			case 0x500A: // es-PR Spanish (Puerto Rico)
			case 0x040B: // fi-FI Finnish (Finland)
			case 0x040C: // fr-FR French (France)
			case 0x080C: // fr-BE French (Belgium)
			case 0x0C0C: // fr-CA French (Canada)
			case 0x100C: // fr-CH French (Switzerland)
			case 0x140C: // fr-LU French (Luxembourg)
			case 0x180C: // fr-MC French (Principality of Monaco)
			case 0x040D: // he-IL Hebrew (Israel)
			case 0x040E: // hu-HU Hungarian (Hungary)
			case 0x040F: // is-IS Icelandic (Iceland)
			case 0x0410: // it-IT Italian (Italy)
			case 0x0810: // it-CH Italian (Switzerland)
			case 0x0411: // ja-JP Japanese (Japan)
			case 0x0412: // ko-KR Korean (Korea)
			case 0x0413: // nl-NL Dutch (Netherlands)
			case 0x0813: // nl-BE Dutch (Belgium)
			case 0x0414: // nb-NO Norwegian (Bokmål) (Norway)
			case 0x0814: // nn-NO Norwegian (Nynorsk) (Norway)
			case 0x0415: // pl-PL Polish (Poland)
			case 0x0016: // pt Portuguese
			case 0x0416: // pt-BR Portuguese (Brazil)
			case 0x0816: // pt-PT Portuguese (Portugal)
			case 0x0418: // ro-RO Romanian (Romania)
			case 0x0419: // ru-RU Russian (Russia)
			case 0x041A: // hr-HR Croatian (Croatia)
			case 0x081A: // Lt-sr-SP Serbian (Latin) (Serbia)
			case 0x0C1A: // Cy-sr-SP Serbian (Cyrillic) (Serbia)
			case 0x041B: // sk-SK Slovak (Slovakia)
			case 0x041C: // sq-AL Albanian (Albania)
			case 0x041D: // sv-SE Swedish (Sweden)
			case 0x081D: // sv-FI Swedish (Finland)
			case 0x041E: // th-TH Thai (Thailand)
			case 0x041F: // tr-TR Turkish (Turkey)
			case 0x0420: // ur-PK Urdu (Islamic Republic of Pakistan)
			case 0x0421: // id-ID Indonesian (Indonesia)
			case 0x0422: // uk-UA Ukrainian (Ukraine)
			case 0x0423: // be-BY Belarusian (Belarus)
			case 0x0424: // sl-SI Slovenian (Slovenia)
			case 0x0425: // et-EE Estonian (Estonia)
			case 0x0426: // lv-LV Latvian (Latvia)
			case 0x0427: // lt-LT Lithuanian (Lithuania)
			case 0x0429: // fa-IR Farsi (Iran)
			case 0x042A: // vi-VN Vietnamese (Viet Nam)
			case 0x042B: // hy-AM Armenian (Armenia)
			case 0x042C: // Lt-az-AZ Azeri (Latin) (Azerbaijan)
			case 0x082C: // Cy-az-AZ Azeri (Cyrillic) (Azerbaijan)
			case 0x042D: // eu-ES Basque (Spain)
			case 0x042F: // mk-MK FYRO Macedonian (Former Yugoslav Republic of Macedonia)
			case 0x0436: // af-ZA Afrikaans (South Africa)
			case 0x0437: // ka-GE Georgian (Georgia)
			case 0x0438: // fo-FO Faeroese (Faeroe Islands)
			case 0x0439: // hi-IN Hindi (India)
			case 0x043E: // ms-MY Malay (Malaysia)
			case 0x083E: // ms-BN Malay (Brunei Darussalam)
			case 0x043F: // kk-KZ Kazakh (Kazakhstan)
			case 0x0440: // ky-KZ Kyrgyz (Kyrgyzstan)
			case 0x0441: // sw-KE Swahili (Kenya)
			case 0x0443: // Lt-uz-UZ Uzbek (Latin) (Uzbekistan)
			case 0x0843: // Cy-uz-UZ Uzbek (Cyrillic) (Uzbekistan)
			case 0x0444: // tt-TA Tatar (Tatarstan)
			case 0x0446: // pa-IN Punjabi (India)
			case 0x0447: // gu-IN Gujarati (India)
			case 0x0449: // ta-IN Tamil (India)
			case 0x044A: // te-IN Telugu (India)
			case 0x044B: // kn-IN Kannada (India)
			case 0x044E: // mr-IN Marathi (India)
			case 0x044F: // sa-IN Sanskrit (India)
			case 0x0450: // mn-MN Mongolian (Mongolia)
			case 0x0456: // gl-ES Galician (Spain)
			case 0x0457: // kok-IN Konkani (India)
			case 0x045A: // syr-SY Syriac (Syria)
			case 0x0465: // div-MV Divehi (Maldives)
			case 0x007F: //  Invariant Language (Invariant Country)
				throw new ArgumentException ("There is no region associated with the Invariant Culture (Culture ID: 0x7F).");
			default:
				throw new ArgumentException ("Culture ID " + culture + " (0x" + culture.ToString ("X4")
							 + ") is not a supported culture.");
			}
		}

		public RegionInfo (string name) {
			switch (name) {
			case "AF": // AFGHANISTAN
				NLS_id = 004;
			case "AL": // ALBANIA
				NLS_id = 008;
			case "DZ": // ALGERIA
				NLS_id = 012;
			case "AS": // AMERICAN SAMOA
				NLS_id = 016;
			case "AD": // ANDORRA
				NLS_id = 020;
			case "AO": // ANGOLA
				NLS_id = 024;
			case "AI": // ANGUILLA
				NLS_id = 660;
			case "AQ": // ANTARCTICA
				NLS_id = 010;
			case "AG": // ANTIGUA AND BARBUDA
				NLS_id = 028;
			case "AR": // ARGENTINA
				NLS_id = 032;
			case "AM": // ARMENIA
				NLS_id = 051;
			case "AW": // ARUBA
				NLS_id = 533;
			case "AU": // AUSTRALIA
				NLS_id = 036;
			case "AT": // AUSTRIA
				NLS_id = 040;
			case "AZ": // AZERBAIJAN
				NLS_id = 031;
			case "BS": // BAHAMAS
				NLS_id = 044;
			case "BH": // BAHRAIN
				NLS_id = 048;
			case "BD": // BANGLADESH
				NLS_id = 050;
			case "BB": // BARBADOS
				NLS_id = 052;
			case "BY": // BELARUS
				NLS_id = 112;
			case "BE": // BELGIUM
				NLS_id = 056;
			case "BZ": // BELIZE
				NLS_id = 084;
			case "BJ": // BENIN
				NLS_id = 204;
			case "BM": // BERMUDA
				NLS_id = 060;
			case "BT": // BHUTAN
				NLS_id = 064;
			case "BO": // BOLIVIA
				NLS_id = 068;
			case "BA": // BOSNIA AND HERZEGOWINA
				NLS_id = 070;
			case "BW": // BOTSWANA
				NLS_id = 072;
			case "BV": // BOUVET ISLAND
				NLS_id = 074;
			case "BR": // BRAZIL
				NLS_id = 076;
			case "IO": // BRITISH INDIAN OCEAN TERRITORY
				NLS_id = 086;
			case "BN": // BRUNEI DARUSSALAM
				NLS_id = 096;
			case "BG": // BULGARIA
				NLS_id = 100;
			case "BF": // BURKINA FASO
				NLS_id = 854;
			case "BI": // BURUNDI
				NLS_id = 108;
			case "KH": // CAMBODIA
				NLS_id = 116;
			case "CM": // CAMEROON
				NLS_id = 120;
			case "CA": // CANADA
				NLS_id = 124;
			case "CV": // CAPE VERDE
				NLS_id = 132;
			case "KY": // CAYMAN ISLANDS
				NLS_id = 136;
			case "CF": // CENTRAL AFRICAN REPUBLIC
				NLS_id = 140;
			case "TD": // CHAD
				NLS_id = 148;
			case "CL": // CHILE
				NLS_id = 152;
			case "CN": // CHINA
				NLS_id = 156;
			case "CX": // CHRISTMAS ISLAND
				NLS_id = 162;
			case "CC": // COCOS (KEELING) ISLANDS
				NLS_id = 166;
			case "CO": // COLOMBIA
				NLS_id = 170;
			case "KM": // COMOROS
				NLS_id = 174;
			case "CG": // CONGO
				NLS_id = 178;
			case "CK": // COOK ISLANDS
				NLS_id = 184;
			case "CR": // COSTA RICA
				NLS_id = 188;
			case "CI": // COTE D'IVOIRE
				NLS_id = 384;
			case "HR": // CROATIA (local name: Hrvatska)
				NLS_id = 191;
			case "CU": // CUBA
				NLS_id = 192;
			case "CY": // CYPRUS
				NLS_id = 196;
			case "CZ": // CZECH REPUBLIC
				NLS_id = 203;
			case "DK": // DENMARK
				NLS_id = 208;
			case "DJ": // DJIBOUTI
				NLS_id = 262;
			case "DM": // DOMINICA
				NLS_id = 212;
			case "DO": // DOMINICAN REPUBLIC
				NLS_id = 214;
			case "TP": // EAST TIMOR
				NLS_id = 626;
			case "EC": // ECUADOR
				NLS_id = 218;
			case "EG": // EGYPT
				NLS_id = 818;
			case "SV": // EL SALVADOR
				NLS_id = 222;
			case "GQ": // EQUATORIAL GUINEA
				NLS_id = 226;
			case "ER": // ERITREA
				NLS_id = 232;
			case "EE": // ESTONIA
				NLS_id = 233;
			case "ET": // ETHIOPIA
				NLS_id = 231;
			case "FK": // FALKLAND ISLANDS (MALVINAS)
				NLS_id = 238;
			case "FO": // FAROE ISLANDS
				NLS_id = 234;
			case "FJ": // FIJI
				NLS_id = 242;
			case "FI": // FINLAND
				NLS_id = 246;
			case "FR": // FRANCE
				NLS_id = 250;
			case "FX": // FRANCE, METROPOLITAN
				NLS_id = 249;
			case "GF": // FRENCH GUIANA
				NLS_id = 254;
			case "PF": // FRENCH POLYNESIA
				NLS_id = 258;
			case "TF": // FRENCH SOUTHERN TERRITORIES
				NLS_id = 260;
			case "GA": // GABON
				NLS_id = 266;
			case "GM": // GAMBIA
				NLS_id = 270;
			case "GE": // GEORGIA
				NLS_id = 268;
			case "DE": // GERMANY
				NLS_id = 276;
			case "GH": // GHANA
				NLS_id = 288;
			case "GI": // GIBRALTAR
				NLS_id = 292;
			case "GR": // GREECE
				NLS_id = 300;
			case "GL": // GREENLAND
				NLS_id = 304;
			case "GD": // GRENADA
				NLS_id = 308;
			case "GP": // GUADELOUPE
				NLS_id = 312;
			case "GU": // GUAM
				NLS_id = 316;
			case "GT": // GUATEMALA
				NLS_id = 320;
			case "GN": // GUINEA
				NLS_id = 324;
			case "GW": // GUINEA-BISSAU
				NLS_id = 624;
			case "GY": // GUYANA
				NLS_id = 328;
			case "HT": // HAITI
				NLS_id = 332;
			case "HM": // HEARD AND MC DONALD ISLANDS
				NLS_id = 334;
			case "VA": // HOLY SEE (VATICAN CITY STATE)
				NLS_id = 336;
			case "HN": // HONDURAS
				NLS_id = 340;
			case "HK": // HONG KONG
				NLS_id = 344;
			case "HU": // HUNGARY
				NLS_id = 348;
			case "IS": // ICELAND
				NLS_id = 352;
			case "IN": // INDIA
				NLS_id = 356;
			case "ID": // INDONESIA
				NLS_id = 360;
			case "IR": // IRAN (ISLAMIC REPUBLIC OF)
				NLS_id = 364;
			case "IQ": // IRAQ
				NLS_id = 368;
			case "IE": // IRELAND
				NLS_id = 372;
			case "IL": // ISRAEL
				NLS_id = 376;
			case "IT": // ITALY
				NLS_id = 380;
			case "JM": // JAMAICA
				NLS_id = 388;
			case "JP": // JAPAN
				NLS_id = 392;
			case "JO": // JORDAN
				NLS_id = 400;
			case "KZ": // KAZAKHSTAN
				NLS_id = 398;
			case "KE": // KENYA
				NLS_id = 404;
			case "KI": // KIRIBATI
				NLS_id = 296;
			case "KP": // KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF
				NLS_id = 408;
			case "KR": // KOREA, REPUBLIC OF
				NLS_id = 410;
			case "KW": // KUWAIT
				NLS_id = 414;
			case "KG": // KYRGYZSTAN
				NLS_id = 417;
			case "LA": // LAO PEOPLE'S DEMOCRATIC REPUBLIC
				NLS_id = 418;
			case "LV": // LATVIA
				NLS_id = 428;
			case "LB": // LEBANON
				NLS_id = 422;
			case "LS": // LESOTHO
				NLS_id = 426;
			case "LR": // LIBERIA
				NLS_id = 430;
			case "LY": // LIBYAN ARAB JAMAHIRIYA
				NLS_id = 434;
			case "LI": // LIECHTENSTEIN
				NLS_id = 438;
			case "LT": // LITHUANIA
				NLS_id = 440;
			case "LU": // LUXEMBOURG
				NLS_id = 442;
			case "MO": // MACAU
				NLS_id = 446;
			case "MK": // MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF
				NLS_id = 807;
			case "MG": // MADAGASCAR
				NLS_id = 450;
			case "MW": // MALAWI
				NLS_id = 454;
			case "MY": // MALAYSIA
				NLS_id = 458;
			case "MV": // MALDIVES
				NLS_id = 462;
			case "ML": // MALI
				NLS_id = 466;
			case "MT": // MALTA
				NLS_id = 470;
			case "MH": // MARSHALL ISLANDS
				NLS_id = 584;
			case "MQ": // MARTINIQUE
				NLS_id = 474;
			case "MR": // MAURITANIA
				NLS_id = 478;
			case "MU": // MAURITIUS
				NLS_id = 480;
			case "YT": // MAYOTTE
				NLS_id = 175;
			case "MX": // MEXICO
				NLS_id = 484;
			case "FM": // MICRONESIA, FEDERATED STATES OF
				NLS_id = 583;
			case "MD": // MOLDOVA, REPUBLIC OF
				NLS_id = 498;
			case "MC": // MONACO
				NLS_id = 492;
			case "MN": // MONGOLIA
				NLS_id = 496;
			case "MS": // MONTSERRAT
				NLS_id = 500;
			case "MA": // MOROCCO
				NLS_id = 504;
			case "MZ": // MOZAMBIQUE
				NLS_id = 508;
			case "MM": // MYANMAR
				NLS_id = 104;
			case "NA": // NAMIBIA
				NLS_id = 516;
			case "NR": // NAURU
				NLS_id = 520;
			case "NP": // NEPAL
				NLS_id = 524;
			case "NL": // NETHERLANDS
				NLS_id = 528;
			case "AN": // NETHERLANDS ANTILLES
				NLS_id = 530;
			case "NC": // NEW CALEDONIA
				NLS_id = 540;
			case "NZ": // NEW ZEALAND
				NLS_id = 554;
			case "NI": // NICARAGUA
				NLS_id = 558;
			case "NE": // NIGER
				NLS_id = 562;
			case "NG": // NIGERIA
				NLS_id = 566;
			case "NU": // NIUE
				NLS_id = 570;
			case "NF": // NORFOLK ISLAND
				NLS_id = 574;
			case "MP": // NORTHERN MARIANA ISLANDS
				NLS_id = 580;
			case "NO": // NORWAY
				NLS_id = 578;
			case "OM": // OMAN
				NLS_id = 512;
			case "PK": // PAKISTAN
				NLS_id = 586;
			case "PW": // PALAU
				NLS_id = 585;
			case "PA": // PANAMA
				NLS_id = 591;
			case "PG": // PAPUA NEW GUINEA
				NLS_id = 598;
			case "PY": // PARAGUAY
				NLS_id = 600;
			case "PE": // PERU
				NLS_id = 604;
			case "PH": // PHILIPPINES
				NLS_id = 608;
			case "PN": // PITCAIRN
				NLS_id = 612;
			case "PL": // POLAND
				NLS_id = 616;
			case "PT": // PORTUGAL
				NLS_id = 620;
			case "PR": // PUERTO RICO
				NLS_id = 630;
			case "QA": // QATAR
				NLS_id = 634;
			case "RE": // REUNION
				NLS_id = 638;
			case "RO": // ROMANIA
				NLS_id = 642;
			case "RU": // RUSSIAN FEDERATION
				NLS_id = 643;
			case "RW": // RWANDA
				NLS_id = 646;
			case "KN": // SAINT KITTS AND NEVIS
				NLS_id = 659;
			case "LC": // SAINT LUCIA
				NLS_id = 662;
			case "VC": // SAINT VINCENT AND THE GRENADINES
				NLS_id = 670;
			case "WS": // SAMOA
				NLS_id = 882;
			case "SM": // SAN MARINO
				NLS_id = 674;
			case "ST": // SAO TOME AND PRINCIPE
				NLS_id = 678;
			case "SA": // SAUDI ARABIA
				NLS_id = 682;
			case "SN": // SENEGAL
				NLS_id = 686;
			case "SC": // SEYCHELLES
				NLS_id = 690;
			case "SL": // SIERRA LEONE
				NLS_id = 694;
			case "SG": // SINGAPORE
				NLS_id = 702;
			case "SK": // SLOVAKIA (Slovak Republic)
				NLS_id = 703;
			case "SI": // SLOVENIA
				NLS_id = 705;
			case "SB": // SOLOMON ISLANDS
				NLS_id = 090;
			case "SO": // SOMALIA
				NLS_id = 706;
			case "ZA": // SOUTH AFRICA
				NLS_id = 710;
			case "GS": // SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS
				NLS_id = 239;
			case "ES": // SPAIN
				NLS_id = 724;
			case "LK": // SRI LANKA
				NLS_id = 144;
			case "SH": // ST. HELENA
				NLS_id = 654;
			case "PM": // ST. PIERRE AND MIQUELON
				NLS_id = 666;
			case "SD": // SUDAN
				NLS_id = 736;
			case "SR": // SURINAME
				NLS_id = 740;
			case "SJ": // SVALBARD AND JAN MAYEN ISLANDS
				NLS_id = 744;
			case "SZ": // SWAZILAND
				NLS_id = 748;
			case "SE": // SWEDEN
				NLS_id = 752;
			case "CH": // SWITZERLAND
				NLS_id = 756;
			case "SY": // SYRIAN ARAB REPUBLIC
				NLS_id = 760;
			case "TW": // TAIWAN, PROVINCE OF CHINA
				NLS_id = 158;
			case "TJ": // TAJIKISTAN
				NLS_id = 762;
			case "TZ": // TANZANIA, UNITED REPUBLIC OF
				NLS_id = 834;
			case "TH": // THAILAND
				NLS_id = 764;
			case "TG": // TOGO
				NLS_id = 768;
			case "TK": // TOKELAU
				NLS_id = 772;
			case "TO": // TONGA
				NLS_id = 776;
			case "TT": // TRINIDAD AND TOBAGO
				NLS_id = 780;
			case "TN": // TUNISIA
				NLS_id = 788;
			case "TR": // TURKEY
				NLS_id = 792;
			case "TM": // TURKMENISTAN
				NLS_id = 795;
			case "TC": // TURKS AND CAICOS ISLANDS
				NLS_id = 796;
			case "TV": // TUVALU
				NLS_id = 798;
			case "UG": // UGANDA
				NLS_id = 800;
			case "UA": // UKRAINE
				NLS_id = 804;
			case "AE": // UNITED ARAB EMIRATES
				NLS_id = 784;
			case "GB": // UNITED KINGDOM
				NLS_id = 826;
			case "US": // UNITED STATES
				NLS_id = 840;
			case "UM": // UNITED STATES MINOR OUTLYING ISLANDS
				NLS_id = 581;
			case "UY": // URUGUAY
				NLS_id = 858;
			case "UZ": // UZBEKISTAN
				NLS_id = 860;
			case "VU": // VANUATU
				NLS_id = 548;
			case "VE": // VENEZUELA
				NLS_id = 862;
			case "VN": // VIET NAM
				NLS_id = 704;
			case "VG": // VIRGIN ISLANDS (BRITISH)
				NLS_id = 092;
			case "VI": // VIRGIN ISLANDS (U.S.)
				NLS_id = 850;
			case "WF": // WALLIS AND FUTUNA ISLANDS
				NLS_id = 876;
			case "EH": // WESTERN SAHARA
				NLS_id = 732;
			case "YE": // YEMEN
				NLS_id = 887;
			case "YU": // YUGOSLAVIA
				NLS_id = 891;
			case "ZR": // ZAIRE
				NLS_id = 180;
			case "ZM": // ZAMBIA
				NLS_id = 894;
			case "ZW": // ZIMBABWE
				NLS_id = 716;
			default:
				throw new ArgumentException ("Region name " + name + " is not supported");
			}
		}

                public virtual string CurrencySymbol {
			get {
				switch (NLS_id) {
				default:
					throw new Exception ();
				}
			}
		}

                public override bool Equals(object value) {
			return value == this;
		}

                public override int GetHashCode () {
			return NLS_id.GetHashCode ();
		}
	}

}
