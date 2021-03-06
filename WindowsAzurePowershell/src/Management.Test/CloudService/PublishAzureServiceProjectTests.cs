﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.Management.Test.CloudService
{
    using System.Management.Automation;
    using System.Net;
    using Microsoft.WindowsAzure.Management.CloudService;
    using Microsoft.WindowsAzure.Management.Test.Utilities.CloudService;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.CloudService;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Moq;
    using ServiceManagement;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PublishAzureServiceTests : TestBase
    {
        private Mock<ICloudServiceClient> clientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        private PublishAzureServiceProjectCommand publishAzureServiceCmdlet;

        private string serviceName = "cloudService";

        [TestInitialize]
        public void SetupTest()
        {
            clientMock = new Mock<ICloudServiceClient>();
            clientMock.Setup(f => f.PublishCloudService(serviceName, null, null, null, null, null, false))
                .Returns(new Deployment());

            commandRuntimeMock = new Mock<ICommandRuntime>();

            publishAzureServiceCmdlet = new PublishAzureServiceProjectCommand()
            {
                CloudServiceClient = clientMock.Object,
                CommandRuntime = commandRuntimeMock.Object
            };
        }

        [TestMethod]
        public void TestPublishAzureService()
        {
            // Setup
            publishAzureServiceCmdlet.ServiceName = serviceName;

            // Test
            publishAzureServiceCmdlet.ExecuteCmdlet();

            // Assert
            clientMock.Verify(f => f.PublishCloudService(serviceName, null, null, null, null, null, false), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<Deployment>()), Times.Once());
        }
    }
}