﻿using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace ToDoGrpc;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(toDoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));

        ToDoItem? toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (toDoItem != null)
        {
            return await Task.FromResult(new ReadToDoResponse
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }
}