using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnnotationsExtensions;


    public class SettingsModel
    {

        public string Username { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }

        public string OldPassword { get; set; }
        public string Password { get; set; }
        [EqualTo("Password")]
        public string Confirm { get; set; }

        
    }

