using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderCoachoutlet.DataClass
{
    internal class OrderResult
    {
        public CardData CardData { get; set; } = new CardData();
        public AddressDataResult AddressDataResult { get; } = new AddressDataResult();

        public NameData NameData { get; } = new NameData();

        public string OrderId { get; set; }

        public override string ToString()
        {
            return $"{OrderId} {NameData.FirstName} {NameData.LastName} {AddressDataResult}";
        }
    }

    internal class AddressDataResult
    {
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public override string ToString()
        {
            return $"{Address} {Address2} {City} {StateCode} {PostalCode} {CountryName} {Email} {Phone}";
        }
    }
}
