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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;

    public static class CredentialHelper
    {
        private static string EnvironmentPathFormat = "testcredentials-{0}";
        private static string defaultCredentialFile = "default.publishsettings";
        private static string TestEnvironmentVariable = "AZURE_TEST_ENVIRONMENT";
        private static string StorageAccountVariable = "AZURE_STORAGE_ACCOUNT";
        private static string StorageAccountKeyVariable = "AZURE_STORAGE_ACCESS_KEY";
        private static string CredentialBlobUriFormat = "https://{0}.blob.core.windows.net";
        public static string CredentialImportFormat = "Import-AzurePublishSettingsFile '{0}'";
        
        private static string publishSettingsFile = null;
        private static string defaultSubscriptionName = null;
        private static string location = "West US";
        private static CloudBlobContainer blobContainer;

        private static string downloadDirectoryPath = null;
        private static Dictionary<string, string> environment = new Dictionary<string, string>();
        public static Dictionary<string, string> PowerShellVariables { get; private set; }

        public static void GetCredentialInfo(string downloadDirectoryPath)
        {
            Process currentProcess = Process.GetCurrentProcess();
            StringDictionary environment = currentProcess.StartInfo.EnvironmentVariables;
            Assert.IsTrue(environment.ContainsKey(TestEnvironmentVariable),
                string.Format("You must define a test environment using environment variable {0}", TestEnvironmentVariable));
            string testEnvironment = environment[TestEnvironmentVariable];
            Assert.IsTrue(environment.ContainsKey(StorageAccountVariable),
                string.Format("You must define a storage account for credential download using environment variable {0}", StorageAccountVariable));
            string storageAccount = environment[StorageAccountVariable];
            Assert.IsTrue(environment.ContainsKey(StorageAccountKeyVariable),
                string.Format("You must define a storage account key for credential download using environment variable {0}", StorageAccountKeyVariable));
            string storageAccountKey = environment[StorageAccountKeyVariable];
            DownloadTestCredentials(testEnvironment, downloadDirectoryPath, 
                string.Format(CredentialBlobUriFormat, storageAccount),
                storageAccount, storageAccountKey);

            publishSettingsFile = Path.Combine(downloadDirectoryPath, defaultCredentialFile);
            Assert.IsTrue(File.Exists(publishSettingsFile), string.Format("Did not download file {0}", publishSettingsFile));
        }

        private static void DownloadTestCredentials(string testEnvironment, string downloadDirectoryPath, string blobUri, string storageAccount, string storageKey)
        {
            string containerPath = string.Format(EnvironmentPathFormat, testEnvironment);
            StorageCredentials credentials = new StorageCredentials(storageAccount, storageKey);
            CloudBlobClient blobClient = new CloudBlobClient(new Uri(blobUri), credentials);
            blobContainer = blobClient.GetContainerReference(containerPath);
            foreach (IListBlobItem blobItem in blobContainer.ListBlobs())
            {
                ICloudBlob blob = blobClient.GetBlobReferenceFromServer(blobItem.Uri);
                Console.WriteLine("Downloading file {0} from blob Uri {1}", blob.Name, blob.Uri);
                FileStream blobStream = new FileStream(Path.Combine(downloadDirectoryPath, blob.Name), FileMode.Create);
                blob.DownloadToStream(blobStream);
                blobStream.Flush();
                blobStream.Close();
            }
        }

        public static void GetTestSettings(string testSettings)
        {
            switch (testSettings)
            {
                case "UseDefaults":
                default:
                    CredentialHelper.GetCredentialInfo(Environment.CurrentDirectory);
                    break;

                case "UseCustom":
                    if (!string.IsNullOrWhiteSpace(Resource.PublishSettingsFile))
                    {
                        publishSettingsFile = Resource.PublishSettingsFile;
                    }
                    else
                    {
                        Assert.IsNotNull(CredentialHelper.PublishSettingsFile);
                    }

                    if (!string.IsNullOrWhiteSpace(Resource.DefaultSubscriptionName))
                    {
                        defaultSubscriptionName = Resource.DefaultSubscriptionName;
                    }
                    break;

                case "UseDefaultsandOverride":
                    CredentialHelper.GetCredentialInfo(Environment.CurrentDirectory);

                    if (!string.IsNullOrWhiteSpace(Resource.PublishSettingsFile))
                    {
                        CredentialHelper.PublishSettingsFile = Resource.PublishSettingsFile;
                    }
                    if (!string.IsNullOrWhiteSpace(Resource.DefaultSubscriptionName))
                    {
                        CredentialHelper.DefaultSubscriptionName = Resource.DefaultSubscriptionName;
                    }

                    break;
            }

            if (!string.IsNullOrWhiteSpace(Resource.Location))
            {
                location = Resource.Location;
            }   
        }

        public static string PublishSettingsFile
        {
            get
            {
                return publishSettingsFile;
            }
            set
            {
                publishSettingsFile = value;
            }
        }

        public static string DefaultSubscriptionName
        {
            get
            {
                return defaultSubscriptionName;
            }
            set
            {
                defaultSubscriptionName = value;
            }
        }

        public static string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

    }
}
