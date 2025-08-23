#!/bin/bash
dotnet ef migrations add AddAdditionalFieldsToIdentityTable
dotnet ef database update