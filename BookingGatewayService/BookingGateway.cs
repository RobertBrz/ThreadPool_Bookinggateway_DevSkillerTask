using BookingGatewayRepository;
using BookingGatewayRepository.Model;
using BookingGatewayService.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookingGatewayService
{
    internal class BookingGateway : IBookingGateway
    {
        public IDBRepository DBRepository { get; set; }
        //int workerThredMin;
        //int IOCPThreadMin;
        //int workerThreadsMax;
        //int IOCPThreadMax;
        static ObservableCollection<ObservableModel> tasks = new ObservableCollection<ObservableModel>();

        public BookingGateway(IDBRepository dBRepository)
        {
            DBRepository = dBRepository;
            //ThreadPool.GetMinThreads(out workerThredMin, out IOCPThreadMin);
            //ThreadPool.GetMaxThreads(out workerThreadsMax, out IOCPThreadMax);
            //ThreadPool.SetMaxThreads(workerThreadsMax, IOCPThreadMax);
            //ThreadPool.SetMinThreads(workerThredMin, IOCPThreadMin);
        }

        public void Booking(string uniqueReference, decimal amount, string transactonTitle, string srcAccountNo, string destAccountNo)
        {
            var transactionData = new TransactionData()
            {
                Amount = amount,
                DestAccountNo = destAccountNo,
                SourceAccountNo = srcAccountNo,
                TransactionTitle = transactonTitle,
                UniqueRef = uniqueReference
            };

            DBRepository.SaveBooking(transactionData);
        }

        public void EndBooking()
        {
            var thisGateWay = tasks.ToList().Find(x => x.Gateway == this);
            if (thisGateWay != null && thisGateWay.isInProgress)
            {
                thisGateWay.isInProgress = false;
                tasks.CollectionChanged -= Tasks_CollectionChanged;
            }
            else
            {
                throw new NoStartBookingException();
            }
        }

        public IList<TransactionStatus> GetBookingStatuses(IList<string> uniqueTransactionRefs)
        {
            if (uniqueTransactionRefs == null || uniqueTransactionRefs.Count == 0) return new List<TransactionStatus>();
            var thisGateWay = tasks.ToList().Find(x => x.Gateway == this);

            var transactionRefs = new string[uniqueTransactionRefs.Count];
            uniqueTransactionRefs.CopyTo(transactionRefs, 0);
            var transactions = DBRepository.GetStatuses(transactionRefs);

            if (thisGateWay == null && transactions.Count() == 0) throw new BookingInProgressException();

            return transactions;
        }

        public void StartBooking()
        {
            var thisGateWay = tasks.ToList().Find(x => x.Gateway == this);

            if (tasks.Any(x => x.isInProgress == true)) throw new BookingInProgressException();
            if (thisGateWay != null)
            {
                if (thisGateWay.isInProgress) throw new BookingInProgressException();
                thisGateWay.isInProgress = true;
            }
            else
            {
                tasks.Add(new ObservableModel(this, true));
            }

            tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collection = sender as ObservableCollection<Task>;
            foreach (var t in collection.Where(t => t.Status == TaskStatus.WaitingForActivation))
            {
                t.Start();
            }
        }
    }
}
