using Microsoft.EntityFrameworkCore;

namespace AsteCore.EF.Model
{
    [Owned]
    public class FSM
    {
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string? MiddleName { get; set; }

        public FSM() { }
        public FSM(string fsm)
        {
            string[] str = fsm.Split(' ');
            FirstName = str[0];
            SecondName = str[1];
            if(str.Length > 2)
                MiddleName = str[2];
        }
        public string ToFullString()
        {
            if (MiddleName != null)
                return $"{FirstName} {SecondName} {MiddleName}";
            else
                return $"{FirstName} {SecondName}";
        }

        public string ToShortString() 
        { 
            if(MiddleName != null)
                return $"{FirstName} {SecondName[0]}.{MiddleName[0]}.";
            else
                return $"{FirstName} {SecondName[0]}";
        }
        

        public override string ToString() => ToFullString();

        public bool Contains(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return false;

            return ToFullString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                   ToShortString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
