using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace Tradera.Statistics
{
    
    public interface IDateService
    {
        string GetCurrentDay();    
    }

    public class DateService : IDateService
    {
        private string toFormat = "yyyy-MM-dd";
        public string GetCurrentDay()
        {
            return System.DateTime.Now.ToString(toFormat);
        }
    }

    public interface IItemStatisticsDataLayer
    {
        void IncrementVipImpressions(int itemId); 
        void IncrementBids(int itemId); 
        void IncrementBuys(int itemId); 
        void IncrementMemoryListAdd(int itemId); 
        void IncreaseSearchImpressions(IEnumerable<int> itemIds); 
    }
    
    public class ItemStatisticsDatalayer : IItemStatisticsDataLayer 
    {
        private const string itemRedisKeyPrefix = "sellerstats:item:";
        private const string searchImpressionHasKey = "searchimpression";
        private const string vipImpressionsHasKey = "vipImpressions";
        private const string memoryListAddsHasKey = "memorylist";
        private const string bidsHasKey = "bids";
        private const string buysHasKey = "buys";
        private const string shopHasKey = "shoppingcart";
        private IConnectionMultiplexer _multiplexer;
        private IDateService _dateService;
        private RedisKey _defaultItemKey;
        
        public ItemStatisticsDatalayer(IConnectionMultiplexer multiplexer, IDateService dateService) 
        {
            _defaultItemKey = new RedisKey().Append(itemRedisKeyPrefix);
            _multiplexer = multiplexer;
            _dateService = dateService;
        }
        
        public void IncrementVipImpressions(int itemId) 
        {
            ExecuteHashIncrement(itemId, vipImpressionsHasKey);
        }
        
        public void IncrementBids(int itemId) 
        {
            ExecuteHashIncrement(itemId, bidsHasKey);
        }
        
        public void IncrementBuys(int itemId) 
        {
            ExecuteHashIncrement(itemId, buysHasKey);
        }
        
        public void IncrementMemoryListAdd(int itemId) 
        {
            ExecuteHashIncrement(itemId, memoryListAddsHasKey);
        }
        
        public void IncreaseSearchImpressions(IEnumerable<int> itemIds) 
        {
            ExecuteCommands(db => {
                var responseTasks = itemIds.Select(itemId => db.HashIncrementAsync(itemRedisKeyPrefix + itemId.ToString(), searchImpressionHasKey, 1)).ToArray();
                db.WaitAll(responseTasks);
                
                if(responseTasks.Any(x=>x.IsFaulted || x.IsCanceled)) 
                {
                    Console.WriteLine("Something went wrong!");
                    foreach (var response in responseTasks)
                    {
                        if (!response.IsCompleted)
                        {
                            Console.WriteLine("Error");
                        }
                    }
                }
            });
        }
        
        private void ExecuteCommands(Action<IDatabase> action)
        {
            var db = _multiplexer.GetDatabase();
            action(db);
        }
        
        private void ExecuteHashIncrement(int itemId, RedisValue hashKey, long value = 1)
        {
            var db = _multiplexer.GetDatabase();
            var counterKey = _defaultItemKey.Append(itemId.ToString());
            var dateCounterKey = _defaultItemKey.Append(itemId.ToString()).Append(":").Append(_dateService.GetCurrentDay());
            var responseCounter = db.HashIncrementAsync(counterKey, hashKey, value);
            var responseDay = db.HashIncrementAsync(dateCounterKey, hashKey, value);
             
            db.Wait(responseCounter);            
            if (responseCounter.IsFaulted)
            {
                System.Console.WriteLine($"Failed to increment {counterKey.ToString()} with value: {memoryListAddsHasKey}");
            }
            
            db.Wait(responseDay);
            if (responseDay.IsFaulted)
            {
                System.Console.WriteLine($"Failed to increment {dateCounterKey.ToString()} with value: {memoryListAddsHasKey}");
            }
        }       
    }
    
}