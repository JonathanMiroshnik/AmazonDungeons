We use the following to connect the Godot folder to VS Code:
https://docs.godotengine.org/en/stable/contributing/development/configuring_an_ide/visual_studio_code.html

To use Amazon Bedrock with .NET:
https://aws.amazon.com/blogs/dotnet/getting-started-with-amazon-bedrock-in-net-applications/

To install NuGet C# packages in VS Code:
https://code.visualstudio.com/docs/csharp/package-management 

Good code examples for .NET AWS Bedrock use:
https://github.com/DennisTraub/bedrock-for-all/tree/main

Amazon Bedrock models that can be used and their model IDs:
https://docs.aws.amazon.com/bedrock/latest/userguide/models-supported.html

For this project, the default Godot .NET version seems to be 6.0(As of 7.12.24) and I changed it to 8.0 in the .cgproj file for more features of C#. 
Godot seems to still be running well with it.

AWS Bedrock is REGION BASED, meaning, if you have access to a model in one region(which you set in the AWS website on the top-right),
it might not transfer access to the model in another region. Also, every region has its own model avaliability of different types.
USEast1 seems to be the "default" region with the most models, and as we aren't interested in high-speed compute and answers, it might be enough for us.