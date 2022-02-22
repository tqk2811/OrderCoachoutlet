using Nito.AsyncEx;
using OrderCoachoutlet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi.ObservableCollection;

namespace OrderCoachoutlet.DataClass
{
    internal class DataManaged
    {
        readonly AsyncLock _mutex = new AsyncLock();
        readonly Random _random = new Random();

        public bool IsAllowRunning
        {
            get { return CardCount > 0 && AddressCount > 0 && NameCount > 0 && ProductCount > 0; }
        }
        public int CardCount => CardDatas.Count;
        public int AddressCount => AddressDatas.Count;
        public int NameCount => NameDatas.Count;
        public int ProxyCount => Proxies.Count;
        public int ProductCount => Products.Count;


        public event Action OnCountChange;



        Queue<CardData> CardDatas { get; } = new Queue<CardData>();
        List<AddressData> AddressDatas { get; } = new List<AddressData>();
        List<NameData> NameDatas { get; } = new List<NameData>();
        List<string> Proxies { get; } = new List<string>();
        List<string> Products { get; } = new List<string>();

        public void LoadCard(string filePath)
        {
            try
            {
                CardDatas.Clear();
                File.ReadAllLines(filePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().Split('|').Select(x => x.Trim()).ToArray())
                    .Where(x => x.Length == 4 && x.All(y => !string.IsNullOrWhiteSpace(y)))
                    .Select(x => new CardData()
                    {
                        CardId = x[0],
                        Month = x[1],
                        Year = x[2],
                        CVV = x[3],
                    })
                    .ToList()
                    .ForEach(x => CardDatas.Enqueue(x));
            }
            catch
            {

            }
            OnCountChange?.Invoke();
        }
        public void LoadAddress(string filePath)
        {
            try
            {
                AddressDatas.Clear();
                AddressDatas.AddRange(File.ReadAllLines(filePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().Split('|').Select(x => x.Trim()).ToArray())
                    .Where(x => x.Length == 5 && x.All(y => !string.IsNullOrWhiteSpace(y)))
                    .Select(x => new AddressData()
                    {
                        Address = x[0],
                        City = x[1],
                        State = x[2],
                        ZipCode = x[3],
                        FirstPhoneNum = x[4],
                        LastPhoneNum = Extensions.RandomNumber(7),
                        Email = $"{Extensions.RandomString(5, 9)}{Extensions.RandomNumber(2, 4)}@gmail.com"
                    }));
            }
            catch
            {

            }
            OnCountChange?.Invoke();
        }
        public void LoadName(string filePath)
        {
            try
            {
                NameDatas.Clear();
                NameDatas.AddRange(File.ReadAllLines(filePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().Split('|').Select(x => x.Trim()).ToArray())
                    .Where(x => x.Length == 2 && x.All(y => !string.IsNullOrWhiteSpace(y)))
                    .Select(x => new NameData()
                    {
                        FirstName = x[0],
                        LastName = x[1],
                    }));
            }
            catch
            {

            }
            OnCountChange?.Invoke();
        }
        public void LoadProxy(string filePath)
        {
            try
            {
                Proxies.Clear();
                Proxies.AddRange(File.ReadAllLines(filePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()));
            }
            catch
            {

            }
            OnCountChange?.Invoke();
        }

        public void LoadProduct(string filePath)
        {
            try
            {
                Products.Clear();
                Products.AddRange(File.ReadAllLines(filePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()));
            }
            catch
            {

            }
            OnCountChange?.Invoke();
        }







        public async Task<CardData> GetNextCard()
        {
            using (await _mutex.LockAsync())
            {
                if (CardDatas.Count > 0)
                {
                    OnCountChange?.Invoke();
                    return CardDatas.Dequeue();
                }
            }
            return null;
        }

        public AddressData GetRamdomAddressData()
        {
            return AddressDatas[_random.Next(AddressDatas.Count)];
        }

        public NameData GetRamdomNameData()
        {
            return new NameData()
            {
                FirstName = NameDatas[_random.Next(NameDatas.Count)].FirstName,
                LastName = NameDatas[_random.Next(NameDatas.Count)].LastName,
            };
        }

        public string GetRandomProxy()
        {
            if (Proxies.Count == 0) return string.Empty;
            return Proxies[_random.Next(Proxies.Count)];
        }

        public IEnumerable<string> GetProducts()
        {
            return Products;
        }
    }
}
