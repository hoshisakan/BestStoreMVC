#!/bin/bash
dotnet ef migrations add AddOrderAndOrderItemsTable
dotnet ef database update