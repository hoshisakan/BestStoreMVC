#!/bin/bash
dotnet ef migrations add AddIdentityTable
dotnet ef database update