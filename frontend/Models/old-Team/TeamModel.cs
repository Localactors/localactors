using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;


    public class TeamModel
    {
        public int TeamId { get; set; }
        public int UserId { get; set; }


        [Required(ErrorMessage = "Inserisci la tua email")]
        [Email(ErrorMessage = "Inserisci un indirizzo email valido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Inserisci una password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Conferma la password")]
        [EqualTo("Password",ErrorMessage = "La password non coincide")]
        public string Confirm { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        public bool Privacy { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        public bool Newsletter { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        public bool TOS { get; set; }

        [Required(ErrorMessage = "Scegli un nome per il team")]
        public string TeamName { get; set; }

        [Required(ErrorMessage = "Seleziona il tuo avatar")]
        public string TeamBadge { get; set; }

        [Required(ErrorMessage = "Inserisci il motto del tuo team")]
        public string TeamMotto { get; set; }
    }
