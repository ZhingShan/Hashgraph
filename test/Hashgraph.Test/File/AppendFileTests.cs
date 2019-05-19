﻿using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class AppendFileContentTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public AppendFileContentTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "File Append: Can Append File Content")]
        public async Task CanAppendToFile()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));
            var concatinatedContent = test.Contents.Concat(appendedContent).ToArray();            

            var appendRecord = await test.Client.AppendFileAsync(new AppendFileParams {
                File = test.CreateRecord.File,
                Contents = appendedContent
            });
            Assert.Equal(ResponseCode.Success, appendRecord.Status);

            var newContent = await test.Client.GetFileContentAsync(test.CreateRecord.File);
            Assert.Equal(concatinatedContent.ToArray(), newContent.ToArray());
        }
        [Fact(DisplayName = "File Append: Append to Deleted File Throws Exception")]
        public async Task AppendingToDeletedFileThrowsError()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);
            var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));

            var deleteRecord = await test.Client.DeleteFileAsync(test.CreateRecord.File);
            Assert.Equal(ResponseCode.Success, deleteRecord.Status);

            var exception = await Assert.ThrowsAnyAsync<TransactionException>(async () => {
                await test.Client.AppendFileAsync(new AppendFileParams
                {
                    File = test.CreateRecord.File,
                    Contents = appendedContent
                });
            });
            Assert.StartsWith("Unable to append to file, status: FileDeleted", exception.Message);
        }
    }
}