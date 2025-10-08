using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using Polly;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine(" --> Consuming AuctionDeleted event:" + context.Message.Id);

        var result = await DB.DeleteAsync<Item>(context.Message.Id);
        
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionDeleted), "Could not delete item in MongoDB");
    }
}
