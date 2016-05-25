using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace Tradera.Statistics
{
    
    public class ItemStatistics {
        
        private IConnectionMultiplexer _multiplexer;
        
        public ItemStatistics(IConnectionMultiplexer multiplexer) {
            _multiplexer = multiplexer;
        }
        
        public void IncrementVipImpressions(int itemId) {
            
        }
        
        public void IncrementBids(int itemId) {
            
        }
        
        public void IncrementBuys(int itemId) {
            
        }
        
        public void IncrementMemoryListAdd(int itemId) {
            
        }
        
        public void IncreaseSearchImpressions(IEnumerable<int> itemIds) {
            var db = _multiplexer.GetDatabase();
            var responseTasks = itemIds.Select(itemId => db.HashIncrementAsync("sellerstats:item:" + itemId.ToString(), "searchimpression", 1)).ToArray();
            db.WaitAll(responseTasks);
            
            if(responseTasks.Any(x=>x.IsFaulted || x.IsCanceled)) {
                Console.WriteLine("Something went wrong!");
                foreach (var response in responseTasks)
                {
                    if (!response.IsCompleted)
                    {
                        Console.WriteLine("Error");
                    }
                }
            }
        }
    }
    
    public class MemberStatistics {
        
    }
    
}