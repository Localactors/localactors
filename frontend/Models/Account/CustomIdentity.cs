using System.Security.Principal;

    public class CustomIdentity : System.Security.Principal.IIdentity
    {
        public static CustomIdentity GetIdentity(string key) {

            string name = key;

            CustomIdentity identity = new CustomIdentity(name);
            return identity;
        }

        public CustomIdentity(string name) {
            Name = name;
        }

        public string Name { get;private set; }
        public string AuthenticationType { get { return "Custom"; } }
        public bool IsAuthenticated {
            get {
                return !string.IsNullOrEmpty(Name);
            }
        }
    }
