using OpenQA.Selenium.Chrome;
using OrderCoachoutlet.DataClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.SeleniumSupport;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using Nito.AsyncEx;
using System.Web;

namespace OrderCoachoutlet.Helpers
{
    internal class ChromeProfile : BaseChromeProfile
    {
        static readonly AsyncLock _mutex = new AsyncLock();
        static readonly Random rnd = new Random();
        readonly string _profileName;
        readonly Action<string> logCallback;
        public ChromeProfile(string profileName, Action<string> logCallback) : base(Singleton.ChromeDriverDir)
        {
            if (string.IsNullOrWhiteSpace(profileName)) throw new ArgumentNullException(nameof(profileName));
            this._profileName = profileName;
            this.logCallback = logCallback;
        }

        void WriteLog(string text)
        {
            logCallback($"{DateTime.Now:HH:mm:ss} :{text}");
        }

        async Task<ChromeOptions> InitChromeOption(string proxy)
        {
            using (await _mutex.LockAsync())
            {
                Singleton.Setting.Setting.ChromeVer =
                await ChromeDriverUpdater.Download(Singleton.ChromeDriverDir, Singleton.Setting.Setting.ChromeVer);
            }

            ChromeOptions chromeOptions = base.DefaultChromeOptions();
            chromeOptions.AddProfilePath(Path.Combine(Singleton.ProfileDir, _profileName));
            if (!string.IsNullOrWhiteSpace(proxy)) chromeOptions.AddProxy(proxy);
            return chromeOptions;
        }
        public async Task OpenChrome(string proxy = null, CancellationToken cancellationToken = default)
        {
            base.OpenChrome(await InitChromeOption(proxy), null, cancellationToken);
        }

        public async Task<OrderResult> Order(CardData cardData, NameData nameData, AddressData addressData)
        {
            if (!IsOpenChrome) throw new InvalidOperationException($"Chrome not open");
            ReadOnlyCollection<IWebElement> eles = null;
            IWebElement ele = null;
            while (true)
            {
                chromeDriver.Navigate().GoToUrl("https://www.coachoutlet.com/");
                WaitUntil(By.TagName("body"), ElementsExists);
                chromeDriver.ExecuteScript("window.scrollTo(0,10000);");
                eles = WaitUntil(By.CssSelector("a[href^='https://www.coachoutlet.com/shop/']"), ElementsExists);
                ele = eles[rnd.Next(eles.Count)];
                JsClick(ele);

                WaitUntil(By.TagName("body"), ElementsExists);
                eles = WaitUntil(By.CssSelector("div.product-tile a[href^='/products/']"), ElementsExists);
                ele = eles[rnd.Next(eles.Count)];
                JsClick(ele);

                WaitUntil(By.TagName("body"), ElementsExists);
                //eles = chromeDriver.FindElements(By.CssSelector("div.select-size button[data-qa='cm_link_size_swatch_enbld']"));
                //if (eles.Count == 0)
                //    continue;
                //ele = eles[rnd.Next(eles.Count)];
                //JsClick(ele);
                await Task.Delay(1000, this.Token);
                ele = WaitUntil(By.CssSelector("button.add-to-cart"), ElementsExists).First();
                if (!ele.Enabled)
                    continue;
                JsClick(ele);
                await Task.Delay(5000, this.Token);
                break;
            }

            chromeDriver.Navigate().GoToUrl("https://www.coachoutlet.com/checkout-begin");
            WaitUntil(By.TagName("body"), ElementsExists);

            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultFirstName']"), ElementsExists)
                .First().SendKeys(nameData.FirstName);
            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultLastName']"), ElementsExists)
                .First().SendKeys(nameData.LastName);
            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultAddressLine1']"), ElementsExists)
                .First().SendKeys(addressData.Address);
            if (!string.IsNullOrWhiteSpace(addressData.Address2))
                WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultAddressLine2']"), ElementsExists)
                    .FirstOrDefault().SendKeys(addressData.Address2);
            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultCity']"), ElementsExists)
                .First().SendKeys(addressData.City);
            ele = WaitUntil(By.CssSelector("form.shipping-form select[aria-describedby='defaultState']"), ElementsExists)
                .First();
            eles = WaitUntil(ele, By.TagName("option"), ElementsExists);
            string val = eles.FirstOrDefault(x => x.Text.Trim().Equals(addressData.State)).GetAttribute("value");
            chromeDriver.ExecuteScript($"arguments[0].value = '{val}';", ele);

            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='defaultZipCode']"), ElementsExists)
                .First().SendKeys(addressData.ZipCode);

            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='emailInvalidMessage']"), ElementsExists)
                .First().SendKeys(addressData.Email);
            WaitUntil(By.CssSelector("form.shipping-form input[aria-describedby='form-phone-error']"), ElementsExists)
                .First().SendKeys(addressData.FirstPhoneNum + addressData.LastPhoneNum);

            WaitUntil(By.CssSelector("button[name='submit'][type='submit']"), ElementsExists)
                .First().Click();

            WaitUntil(By.TagName("body"), ElementsExists);

            using (var frame = FrameSwitch(WaitUntil(By.CssSelector("div.adyen-checkout__field--cardNumber iframe.js-iframe"), ElementsExists).First()))
            {
                WaitUntil(By.CssSelector("input#encryptedCardNumber"), ElementsExists)
                    .First().SendKeys(cardData.CardId);
            }
            using (var frame = FrameSwitch(WaitUntil(By.CssSelector("div.adyen-checkout__field--expiryDate iframe.js-iframe"), ElementsExists).First()))
            {
                WaitUntil(By.CssSelector("input#encryptedExpiryDate"), ElementsExists)
                    .First().SendKeys($"{cardData.Month}/{cardData.Year}");
            }
            using (var frame = FrameSwitch(WaitUntil(By.CssSelector("div.adyen-checkout__field--securityCode iframe.js-iframe"), ElementsExists).First()))
            {
                WaitUntil(By.CssSelector("input#encryptedSecurityCode"), ElementsExists)
                    .First().SendKeys(cardData.CVV);
            }

            WaitUntil(By.CssSelector("button[name='submit'][type='submit'][data-qa='cbp_btn_reviewurorder']"), ElementsExists)
                .First().Click();

            WaitUntil(By.CssSelector("button[name='submit'][type='submit'][data-qa='d_cbr_btn_submiturorder']"), ElementsExists)
                .First().Click();

            WaitUntil(By.TagName("body"), ElementsExists);
            if (chromeDriver.Url.StartsWith("https://www.coachoutlet.com/order-confirmation"))
            {
                OrderResult orderResult = new OrderResult();

                orderResult.NameData.FirstName = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.userName span.firstName"))
                        .FirstOrDefault()?.Text;
                orderResult.NameData.LastName = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.userName span.lastName"))
                        .FirstOrDefault()?.Text;

                orderResult.AddressDataResult.Address = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.address-lines span.address1"))
                        .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.Address2 = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.address-lines span.address2"))
                        .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.City = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.address-details span.city"))
                        .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.StateCode = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.address-details span.stateCode"))
                        .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.PostalCode = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section p.address-details span.postalCode"))
                       .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.CountryName = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-section span.countryName"))
                       .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.Email = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-contact p.order-email"))
                      .FirstOrDefault()?.Text;
                orderResult.AddressDataResult.Phone = chromeDriver.FindElements(By.CssSelector("div.shipping-summary-contact p.shipping-phone"))
                      .FirstOrDefault()?.Text;

                orderResult.OrderId = HttpUtility.ParseQueryString(chromeDriver.Url)["ID"];

                return orderResult;
            }
            else return null;
        }
    }
}
