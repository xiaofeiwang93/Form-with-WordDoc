namespace YourNameSpace.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class YourReportFormModel
    {
        [Required(ErrorMessage ="Please select a Report Type")]
        [DisplayName("Report Type *")]
        public string ReportType { get; set; }

        [DisplayName("Report Date *")]
        public string ReportDate { get; set; }


        // Referee Details
        [Required(ErrorMessage = "Please provide Referee Type")]
        [DisplayName("Referee Type *")]
        public string RefereeType { get; set; }

        [Required(ErrorMessage = "Please provide Referee Name")]
        [DisplayName("Referee Name *")]
        public string RefereeName { get; set; }

        [Required(ErrorMessage = "Please provide Referee Union")]
        [DisplayName("Referee Union *")]
        public string RefereeUnion { get; set; }

        [Required(ErrorMessage = "Please provide an Email")]
        [DisplayName("Referee Email *")]
        [EmailAddress]
        public string RefereeEmail { get; set; }

        [Required(ErrorMessage = "Please provide Referee phone number")]
        [DisplayName("Referee Phone *")]
        public string RefereePhone { get; set; }


        // Player Details 
        [Required(ErrorMessage = "Please provide Player's Full Name")]
        [DisplayName("Player's Full Name *")]
        public string PlayerFullName { get; set; }

        [DisplayName("Player's Club *")]
        public string PlayerUnion { get; set; }

        [DisplayName("Date of Dismissal *")]
        public string DismissalDate { get; set; }

        [Required(ErrorMessage = "Please select Playing Position")]
        [DisplayName("Playing Position *")]
        public string PlayingPosition { get; set; }

        [Required(ErrorMessage = "Please provide Player's Number")]
        [DisplayName("Player's Number *")]
        public string PlayerNumber { get; set; }

        [DisplayName("Team Name/Club/School *")]
        public string TeamName { get; set; }


        // Fixture Details
        [Required(ErrorMessage = "Please provide a Date")]
        [DisplayName("Date *")]
        public string FixtureDate { get; set; }

        [Required(ErrorMessage = "Please provide a Venue")]
        [DisplayName("Venue *")]
        public string Venue { get; set; }

        [Required(ErrorMessage = "Please select Grade")]
        [DisplayName("Grade *")]
        public string Grade { get; set; }

        [Required(ErrorMessage = "Please provide Home Team")]
        [DisplayName("Home Team *")]
        public string HomeTeam { get; set; }

        [Required(ErrorMessage = "Please provide Visitors")]
        [DisplayName("Visitors *")]
        public string Visitors { get; set; }

        [Required(ErrorMessage = "Please provide Final Score")]
        [DisplayName("Final Score (Pts) *")]
        public string FinalScore { get; set; }


        // Nature of Offence
        [Required(ErrorMessage = "Please select Nature of Offence")]
        [DisplayName("Offence *")]
        public string Offence { get; set; }

        [DisplayName("Prior Individual Caution *")]
        public string PriorIndividualCaution { get; set; }

        [DisplayName("Prior General Warning *")]
        public string PriorGeneralWarning { get; set; }

        [DisplayName("Was the player ordered off further to the report of an assistant referee? *")]
        public string ReportToAR { get; set; }

        [Required(ErrorMessage = "Please provide Proximity in metres")]
        [DisplayName("Referees Promximity to Incident (Metres) *")]
        public string RefereesProximity { get; set; }

        [Required(ErrorMessage = "Please provide Time")]
        [DisplayName("Time Elapsed *")]
        public string TimeElapsed { get; set; }

        [Required(ErrorMessage = "Please select Period")]
        [DisplayName("Period (of game when incident occurred) *")]
        public string Period { get; set; }

        [Required(ErrorMessage = "Please provide Score at time")]
        [DisplayName("Score at time (Pts) *")]
        public string Score { get; set; }

        [Required(ErrorMessage = "Please provide detailed report")]
        [DisplayName("Referee's Report *")]
        public string RefereesReport { get; set; }
    }

}