using Nito.AsyncEx;
using OrderCoachoutlet.DataClass;
using OrderCoachoutlet.Helpers;
using OrderCoachoutlet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;

namespace OrderCoachoutlet.Queues
{
    internal class WorkQueue : IQueue
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static readonly AsyncLock _mutex = new AsyncLock();
        readonly DataManaged dataManaged;
        readonly Action<string> logCallback;
        public WorkQueue(DataManaged dataManaged, Action<string> logCallback)
        {
            this.dataManaged = dataManaged ?? throw new ArgumentNullException(nameof(dataManaged));
            this.logCallback = logCallback ?? throw new ArgumentNullException(nameof(logCallback));
        }

        #region IQueue
        public bool IsPrioritize => false;

        public bool ReQueue => false;

        public bool ReQueueAfterRunComplete => false;

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }
        #endregion

        void WriteLog(string text)
        {
            logCallback($"{DateTime.Now:HH:mm:ss} :{text}");
        }

        public async Task DoWork()
        {
            try
            {
                while (true)
                {
                    CardData cardData = await dataManaged.GetNextCard();
                    if (cardData == null) return;
                    try
                    {
                        WriteLog($"Start Card: {cardData.CardId}");
                        ChromeProfile chromeProfile = new ChromeProfile(cardData.CardId, logCallback);

                        OrderResult orderResult = null;
                        while (true)
                        {
                            try
                            {
                                string proxy = dataManaged.GetRandomProxy();
                                WriteLog($"Card {cardData.CardId} : Open chrome ({proxy})");
                                await chromeProfile.OpenChrome(proxy, cancellationTokenSource.Token);

                                orderResult = await chromeProfile.Order(
                                    cardData,
                                    dataManaged.GetProducts(),
                                    dataManaged.GetRamdomNameData(),
                                    dataManaged.GetRamdomAddressData());
                                break;
                            }
                            catch (OperationCanceledException oce)
                            {
                                WriteLog("Cancel");
                                return;
                            }
                            catch (Exception ex)
                            {
                                WriteLog($"{ex.GetType().FullName}: {ex.Message} {ex.StackTrace}");
                                continue;
                            }
                            finally
                            {
                                chromeProfile.CloseChrome();
                            }
                        }
                        if (orderResult != null)
                        {
                            using (var l = await _mutex.LockAsync())
                            {
                                using var stream = File.AppendText(Path.Combine(Singleton.ResultDir, $"{DateTime.Now:yyyy-MM-dd}.txt"));
                                stream.WriteLine(orderResult);
                                WriteLog($"Save {orderResult}");
                            }
                        }
                    }
                    catch (OperationCanceledException oce)
                    {
                        WriteLog("Cancel");
                        return;
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"{ex.GetType().FullName}: {ex.Message} {ex.StackTrace}");
                    }
                    finally
                    {
                        string cardId = cardData.CardId;
                        _ = Task.Delay(5000).ContinueWith(t =>
                            {
                                try { Directory.Delete(Path.Combine(Singleton.ProfileDir, cardId), true); } catch { }
                            });
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                WriteLog("Cancel");
                return;
            }
            catch (Exception ex)
            {
                WriteLog($"{ex.GetType().FullName}: {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
