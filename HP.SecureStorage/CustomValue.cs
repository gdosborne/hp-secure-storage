using System;

namespace HP.SecureStorage {

    // Any custom classes to be stored must contain a static Parse method and take a string as the parameter
    // and the ToString method must be overridden and return the value used by the Parse method

    public class CustomValue {
        public static CustomValue Parse(string value) {
            var parts = value.Split('|');
            if(parts.Length == 3) {
                return new CustomValue {
                    Name = parts[0],
                    Address = parts[1],
                    City = parts[2]
                };
            }else{
                throw new ApplicationException("Must be Name,Address,City values");
            }
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        
        public override string ToString() {
            return $"{Name}|{Address}|{City}";
        }
    }
}
