using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using static Pets.Animal;

namespace Pets
{
   [ServiceContract]
    public interface jla
    {

        //   [WebInvoke(Method = "GET")]
        // string GetTEST();

        //   [OperationContract]
        // string GetData(int value);

        //     [OperationContract]
        //   string GetDat2a(int value);


        //        [OperationContract]
        //        string UpdateAnimalxxxxxxxxxxxxxxxxxxx(int id,
        //      int availableForAdoption_accepts_null = int.MinValue,
        //   int isEnergetic_accepts_null = int.MinValue,
        //   int sterilizationMarked = int.MinValue,
        //    int fleaCollarAlsoForTicks_accepts_null = int.MinValue,
        //     int houseTrained_accepts_null = int.MinValue,
        //   int goodWithChildren_accepts_null = int.MinValue,
        //int goodWithOtherAnimalsSameSpecies_accepts_null = int.MinValue,
        //int goodWithOtherAnimalsDifferentSpecies_accepts_null = int.MinValue,
        //int canLiveOutdoors_accepts_null = int.MinValue,
        //int canBeChained_accepts_null = int.MinValue,
        //    int died_accepts_null = int.MinValue,
        //         int species_ = int.MinValue,
        //     string name = "optional",
        //    string remarks = "optional",
        //string medicalInfo = "optional",
        //  int currentLocation_ = int.MinValue,
        //    DateTime inLocationSince_accepts_null = new DateTime(),
        //    DateTime inLocationUntil_accepts_null = new DateTime(),
        //   DateTime birthDate_accepts_null = new DateTime(),
        //    DateTime arrivalDate_accepts_null = new DateTime(),
        //      int gender_ = int.MinValue,
        //   DateTime fleaCollarPlacedOn_accepts_null = new DateTime(),
        //    string fleaCollarType = "opotional",
        //    int fleaCollarGoodForMonths = int.MinValue,
        //   string cage = "optional",
        //     string color = "optional",
        //    string breed = "optional",
        //    int size_ = int.MinValue,
        //    int weightInKilo = int.MinValue,
        //    DateTime weightMeasuredOn_accepts_null = new DateTime()
        //   );

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string GetToken(Stream stream);

        //      [OperationContract]
        //    string GetTime();

        //        [OperationContract]
        //      int GetDataUsingDataContract(Animal composite);

        /////////////////////////////////////
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetAnimals();

        //    [OperationContract]
        //  string UptateAnimal(Animal composite);

         [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetPersons();

        //        [OperationContract]
        //      string UpdatePerson(Person person);

        /////////////////////////////////////
        //       [OperationContract]
        //     string UpdateAdoption(Adoption adoption);

        //        [OperationContract]
        //      string InsertAdoption(Adoption adoption);

        //      [OperationContract]
        //    string DeleteAdoption(int intId);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetAdoptions();

        /////////////////////////////////////
        //       [OperationContract]
        //     string InsertChip(Chip chip);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetChips();

        //       [OperationContract]
        //     string DeleteChip(int intId);

        //        [OperationContract]
        //      string UpdateChip(Chip chip);

        /////////////////////////////////////
        //      [OperationContract]
        //     string InsertDoc(Doc doc);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetDocs();

        //        [OperationContract]
        //      string DeleteDoc(int intId);

        //     [OperationContract]
        //   string UpdateDoc(Doc doc);
        /////////////////////////////////////
        //    [OperationContract]
        //  string InsertPic(Pic pic);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetPics();

        //        [OperationContract]
        //       string DeletePic(int intId);

        //      [OperationContract]
        //    string UpdatePic(Pic pic);
        /////////////////////////////////////
        //[OperationContract]
        //   string InsertTreatment(Treatment treatment);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetTreatments();

        //        [OperationContract]
        //      string DeleteTreatment(int intId);

        //    [OperationContract]
        //   string UpdateTreatment(Treatment treatment);
        ///////////////////////////////////// 
        //       [OperationContract]
        //     string InsertTreatmentType(TreatmentType treatment);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetTreatmentTypes();

        //      [OperationContract]
        //     string DeleteTreatmentType(int intId);

        //   [OperationContract]
        // string UpdateTreatmentType(TreatmentType treatment);
        ///////////////////////////////////// 
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetGenders();
        /////////////////////////////////////  [OperationContract]
        //      string InsertLocation(Location location);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetLocations();

        //       [OperationContract]
        //     string DeleteLocation(int intId);

        //        [OperationContract]
        //      string UpdateLocation(Location location);
        /////////////////////////////////////  [OperationContract]

        /////////////////////////////////////  [OperationContract]
        //        string InsertSize(Size Size);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetSizes();

        //       [OperationContract]
        //     string DeleteSize(int intId);

        //        [OperationContract]
        //      string UpdateSize(Size size);
        ///////////////////////////////////// 
        //[OperationContract]
        //   string InsertSpecies(Species species);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Message GetSpecies();

        //      [OperationContract]
        //    string UpdateSpecies(Species species);

        //  [OperationContract]
        /// [WebInvoke(Method = "POST", UriTemplate = "Ping")]
        // string Ping(string input);



        /////////////////////////////////////  [OperationContract]    

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteDoc(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteAnimal(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteTreatment(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteTreatmentType(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeletePic(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeletePerson(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteLocation(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteChip(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteAdoption(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteDocumentType(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string DeleteSize(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateTreatmentType(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateChip(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateDoc(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdatePic(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateTreatment(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateLocation(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateSize(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdatePerson(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateAnimal(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateAdoption(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateDocumentType(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertAnimal(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertPerson(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertChip(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertDoc(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertDocumentType(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertLocation(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertPic(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertSize(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertSpecies(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertTreatment(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InsertTreatmentType(Stream stream);
        
        /////////////////////////////////////
        //    [OperationContract]
        //  string InsertPerson(Person person);

    }

    [DataContract]
    public class Animal
    {
        [DataMember]
        public int? id { get; set; }         //int

        [DataMember]
        public bool? goodWithChildren { get; set; }   //bool   

        [DataMember]
        public DateTime? arrivalDate { get; set; }   //DateTime

        [DataMember]
        public string name { get; set; }          //string
        ////    to stop the null string from causing problem later,you can:
        //{
        //    get
        //    {
        //        return _name;
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            _name = "the_user_left_this_blank";
        //        }
        //        else
        //        {
        //            _name = value;
        //        }
        //        name = _name;
        //    }
        //}

        [DataMember]
        public string color { get; set; }

        [DataMember]
        public int? species_ { get; set; }

        [DataMember]
        public string breed { get; set; }

        [DataMember]
        public bool? availableForAdoption { get; set; }

        [DataMember]
        public int? location_ { get; set; }

        [DataMember]
        public DateTime? inLocationSince { get; set; }

        [DataMember]
        public DateTime? inLocationUntil { get; set; }

        [DataMember]
        public int? gender_ { get; set; }

        [DataMember]
        public int? size_ { get; set; }

        [DataMember]
        public decimal? weightInKilo { get; set; }

        [DataMember]
        public DateTime? weightMeasuredOn { get; set; }

        [DataMember]
        public bool? isEnergetic { get; set; }

        [DataMember]
        public DateTime? birthDate { get; set; }

        [DataMember]
        public bool? sterilizationMarked { get; set; }

        [DataMember]
        public DateTime? fleaCollarPlacedOn { get; set; }


        [DataMember]
        public string fleaCollarType { get; set; }

        [DataMember]
        public int? fleaCollarGoodForMonths { get; set; }

        [DataMember]
        public bool? fleaCollarAlsoForTicks { get; set; }

        [DataMember]
        public bool? houseTrained { get; set; }

        [DataMember]
        public bool? goodWithOtherAnimalsSameSpecies { get; set; }

        [DataMember]
        public bool? goodWithOtherAnimalsDifferentSpecies { get; set; }

        [DataMember]
        public bool? canLiveOutdoors { get; set; }

        [DataMember]
        public bool? canBeChained { get; set; }

        [DataMember]
        public string medicalInfo { get; set; }

        [DataMember]
        public bool? died { get; set; }

        [DataMember]
        public string remarks { get; set; }

        [DataMember]
        public string cage { get; set; }


        ///////////////////////////////////////////////////////

    }

    [DataContract]
    public class Person
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string firstName;

        [DataMember]
        public string lastName;

        [DataMember]
        public int? role_;

        public int? gender_;

        [DataMember]
        public int? idCard;

        [DataMember]
        public string email;

        [DataMember]
        public string fax;

        [DataMember]
        public string postOfficeBox;

        [DataMember]
        public string address;

        [DataMember]
        public string phone1;

        [DataMember]
        public string phone2;

        [DataMember]
        public string workPlace;

        [DataMember]
        public bool? everReturnedAnAnimal;

        [DataMember]
        public bool? doNotGiveHimAnimals;

        [DataMember]
        public string remarks;
    }

    [DataContract]
    public class Adoption
    {
        [DataMember]
        public int? id;

        [DataMember]
        public int? animal_;

        [DataMember]
        public int? person_;

        [DataMember]
        public DateTime? adoptedOn;

        [DataMember]
        public DateTime? temporaryUntil;

        [DataMember]
        public string staffMember;

        [DataMember]
        public string remarks;
    }

    [DataContract]
    public class Chip
    {
        [DataMember]
        public int? id;

        [DataMember]
        public long? description;

        [DataMember]
        public int? animal_;

        [DataMember]
        public string chipType;

        [DataMember]
        public string chipLocation;

        [DataMember]
        public string chipPlacedBy;

        [DataMember]
        public DateTime? chipPlacedOn;
    }

    [DataContract]
    public class Doc
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string nameAndPathIncludingExtension;

        [DataMember]
        public int? animal_;

        [DataMember]
        public int? documentType_;

        [DataMember]
        public int? adoption_;

        [DataMember]
        public int? treatment_;

        [DataMember]
        public string remarks;
    }

    [DataContract]
    public class Pic
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string nameAndPathIncludingExtension;

        [DataMember]
        public int? animal_;

        [DataMember]
        public int? adoption_;

        [DataMember]
        public int? treatment_;

        [DataMember]
        public bool? isArrivalPic;

        [DataMember]
        public DateTime? takenOn;

        [DataMember]
        public bool? hiddenFromPublic;

        [DataMember]
        public string remarks;
    }

    [DataContract]
    public class Treatment
    {
        [DataMember]
        public int? id;

        [DataMember]
        public int? animal_;

        [DataMember]
        public int? treatmentType_;

        [DataMember]
        public DateTime? treatmentTime;

        [DataMember]
        public int? person_;

        [DataMember]
        public string medicineName;

        [DataMember]
        public int? medicineDosage;

        [DataMember]
        public int? thisIsRoundNumber;

        [DataMember]
        public DateTime? nextRoundOn;

        [DataMember]
        public string remarks;
    }

    [DataContract]
    public class TreatmentType
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;

        [DataMember]
        public int? numberOfRoundsNeeded;

        [DataMember]
        public int? daysBetweenRounds;
    }

    [DataContract]
    public class DocumentType
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }

    [DataContract]
    public class Genders
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }

    [DataContract]
    public class Location
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }

    [DataContract]
    public class Role
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }

    [DataContract]
    public class Size
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }


    [DataContract]
    public class Species
    {
        [DataMember]
        public int? id;

        [DataMember]
        public string description;
    }
    //    get
    //    {
    //        return _fleaCollarPlacedOn;
    //    }
    //    set 
    //    {
    //        if (value == null)
    //        {
    //            _fleaCollarPlacedOn = null;
    //        }
    //        else
    //        {
    //            string[] strarrDateParts;
    //            strarrDateParts = value.ToString().Split(new char[] { '/' },StringSplitOptions.RemoveEmptyEntries);
    //            if (strarrDateParts.Length < 2)
    //            {
    //                strarrDateParts = value.ToString().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
    //            }
    //            if (strarrDateParts.Length == 2)
    //            {
    //                _fleaCollarPlacedOn = new DateTime(DateTime.Now.Year, int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[0]));
    //            }
    //            else
    //            {
    //                if (int.Parse(strarrDateParts[0]) > 12)
    //                {
    //                    _fleaCollarPlacedOn = new DateTime(int.Parse(strarrDateParts[0]), int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[2]));
    //                }
    //                else
    //                {
    //                    _fleaCollarPlacedOn = new DateTime(int.Parse(strarrDateParts[2]), int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[0]));
    //                }
    //            }
    //        }
    //    }
}
