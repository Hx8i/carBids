using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Item>> searchItems([FromQuery] SearchParms searchParms)
    {
        var searchTerm = searchParms.SearchTerm;
        var pageNumber = searchParms.PageNumber;
        var pageSize = searchParms.PageSize;
        var seller = searchParms.Seller;
        var winner = searchParms.Winner;
        var orderBy = searchParms.OrderBy;
        var filterBy = searchParms.FilterBy;

        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }

        query = orderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Ascending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };

        query = filterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "ending" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if(!string.IsNullOrEmpty(seller))
        {
            query = query.Match(x => x.Seller == seller);
        }

        if(!string.IsNullOrEmpty(winner))
        {
            query = query.Match(x => x.Winner == winner);
        }

        query.PageNumber(pageNumber);
        query.PageSize(pageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            result = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount,
        });
    }
}
