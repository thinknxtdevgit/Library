using System.ComponentModel.DataAnnotations;

namespace lib.Models
{
    public class Admissions
    {
      
            // ===== BASIC =====
            public string Session { get; set; }
            public string CollegeName { get; set; }
            public string Course { get; set; }
            public string Class { get; set; }
            public string Batch { get; set; }
            public string Semester { get; set; }
            public string IDNo { get; set; }
            public string ClassRollNo { get; set; }

            // ===== STUDENT TYPE =====
            public string StudentType { get; set; } // New / Old
            public string Gender { get; set; }
            public string Locality { get; set; }

            // ===== PERSONAL =====
            public string StudentName { get; set; }
            public string GrandFatherName { get; set; }
            public string FatherName { get; set; }
            public string MotherName { get; set; }
            public string BloodGroup { get; set; }
            public string AadharNo { get; set; }

            // ===== CONTACT =====
            public string StudentMobileNo { get; set; }
            public string FatherMobileNo { get; set; }
            public string MotherMobileNo { get; set; }
            public string Email { get; set; }
            public string FatherEmail { get; set; }

            // ===== ADDRESS =====
            public string CorrespondanceAddress { get; set; }
            public string PermanentAddress { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string District { get; set; }
            public string Tehsil { get; set; }
            public string VPO { get; set; }
            public string PO { get; set; }

            // ===== FAMILY =====
            public string FatherOccupation { get; set; }
            public string MotherOccupation { get; set; }
            public string GuardianName { get; set; }
            public string GuardianAddress { get; set; }
            public string GuardianContactNo { get; set; }
            public string GuardianRelation { get; set; }

            // ===== ACADEMIC =====
            public string SubjectComb { get; set; }
            public string SubjectComb2 { get; set; }
            public string Optional { get; set; }
            public string Optional2 { get; set; }
            public string LastExam { get; set; }
            public string LastExamPerc { get; set; }
            public string EnquiryMode { get; set; }
            public string Board { get; set; }
            public string Section { get; set; }
            public string Shift { get; set; }

            // ===== DATES =====
            public string AdmissionDate { get; set; }
            public string DOB { get; set; }
            public string EnquiryDate { get; set; }
            public string RegistrationDate { get; set; }

            // ===== ENQUIRY / REG =====
            public string EnquiryNo { get; set; }
            public string RegistrationNo { get; set; }

            // ===== EXTRA =====
            public string Nationality { get; set; }
            public string Religion { get; set; }
            public string Category { get; set; }
            public string ModeOfAdmission { get; set; } // Quota
            public string Scheme { get; set; }
            public string LateralEntry { get; set; }

            // ===== FACILITY =====
            public string Facility { get; set; } // Bus / Hostel / None
            public string HostelName { get; set; }
            public string RoomType { get; set; }
            public string RouteID { get; set; }
            public string StopageID { get; set; }
            public string BusRoute { get; set; }
            public string Stopage { get; set; }
            public string HostelCharges { get; set; }
            public string BoardRegistrationNo { get; set; }
            public string BusFee { get; set; }
            public string ValidUpTo { get; set; }
        public string ConcessionReferenceLetterNo { get; set; }
        public string FirstPreference { get; set; }
        public string SecondPreference { get; set; }
        public string ThirdPreference { get; set; }
        public string FourthPreference { get; set; }
        // ===== CONCESSION =====
        public string Concession { get; set; }
            public string ConcessionDetails { get; set; }
            public string ConcessionPerc { get; set; }
            public string ConcessionTotalAmount { get; set; }
            public string CurrentConcession { get; set; }

            // ===== TEST =====
            public string EntranceTest1 { get; set; }
            public string EntranceTest1Rank { get; set; }
            public string EntranceTest1RollNo { get; set; }

            public string EntranceTest2 { get; set; }
            public string EntranceTest2Rank { get; set; }
            public string EntranceTest2RollNo { get; set; }

            // ===== BANK =====
            public string AcName { get; set; }
            public string BankName { get; set; }
            public string BankBranch { get; set; }
            public string AccNo { get; set; }
            public string IFSCCode { get; set; }

            // ===== REFERENCE =====
            public string Reference { get; set; }
            public string Description { get; set; }

            // ===== OTHER =====
            public string OtherAchievements { get; set; }
            public string Sports { get; set; }
            public string NSS { get; set; }
            public string PreviousMedicalIllness { get; set; }
            public string InstitutionLastAttended { get; set; }

            // ===== RELATIVE =====
            public string Relative { get; set; }
            public string RInstitution { get; set; }
            public string RCourse { get; set; }
            public string RRollNo { get; set; }

            public string MotherOfficeAddress { get; set; }
            public string Adm_Category { get; set; }
            //public string CompleteSession { get; set; }

            // ===== USER =====
            public string UserID { get; set; }

            // ===== DEBIT =====
            public List<DebitFeeModel> DebitFees { get; set; }
            public string TotalDebit { get; set; }

            // ===== QUALIFICATION GRID =====
            public List<EduQualificationModel> Qualifications { get; set; }
        }
}
