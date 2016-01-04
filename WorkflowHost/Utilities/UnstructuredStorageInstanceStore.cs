using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.Utilities
{
    using System.Activities.DurableInstancing;
    using System.Activities.Hosting;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;

    using WorkflowHost.DataStorage;
    using WorkflowHost.Entities;

    internal class UnstructuredStorageInstanceStore : InstanceStore
    {
        #region Static Fields

        /// <summary>
        ///     The local lock.
        /// </summary>
        private static readonly object LocalLock = new object();

        #endregion

        #region Fields

        /// <summary>
        ///     The submit state message.
        /// </summary>
        private readonly Action<Guid> submitStateMessage;

        /// <summary>
        ///     The unstructured storage repository.
        /// </summary>
        private readonly AzureTableStorageRepository<InstanceData> unstructuredStorageRepository;

        /// <summary>
        ///     The owner instance id.
        /// </summary>
        private Guid ownerInstanceId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnstructuredStorageInstanceStore"/> class.
        /// </summary>
        /// <param name="unstructuredStorageRepository">
        /// The unstructured storage repository.
        /// </param>
        /// <param name="ownerInstanceId">
        /// The owner instance id.
        /// </param>
        /// <param name="submitStateMessage">
        /// The submit State Message.
        /// </param>
        internal UnstructuredStorageInstanceStore(
            AzureTableStorageRepository<InstanceData> unstructuredStorageRepository,
            Guid ownerInstanceId,
            Action<Guid> submitStateMessage)
        {
            lock (LocalLock)
            {
                this.unstructuredStorageRepository = unstructuredStorageRepository;
                this.ownerInstanceId = ownerInstanceId;
                this.submitStateMessage = submitStateMessage;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The begin try command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <returns>
        /// The <see cref="IAsyncResult"/>.
        /// </returns>
        /// <exception cref="SystemFailureException">
        /// Error processing message.
        /// </exception>
        protected override IAsyncResult BeginTryCommand(
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            Contract.Requires<Exception>(null != context, "context");
            lock (LocalLock)
            {
                if (command is CreateWorkflowOwnerCommand)
                {
                    context.BindInstanceOwner(this.ownerInstanceId, Guid.NewGuid());
                }
                else
                {
                    var saveWorkflowCommand = command as SaveWorkflowCommand;
                    if (saveWorkflowCommand != null)
                    {
                        this.SaveInstanceData(saveWorkflowCommand.InstanceData);
                    }
                    else if (command is LoadWorkflowCommand)
                    {
                        try
                        {
                            var instanceData =
                                this.unstructuredStorageRepository.GetById(
                                    "WorkflowInstanceStoreData",
                                    this.ownerInstanceId.ToString());
                            var deserializedData = DeserializeData(instanceData.Payload.Combine());
                            context.LoadedInstance(InstanceState.Initialized, deserializedData, null, null, null);
                        }
                        catch (Exception exception)
                        {
                            throw;
                        }
                    }
                }

                return new CompletedAsyncResult<bool>(true, callback, state);
            }
        }

        /// <summary>
        /// The end try command.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        /// <summary>
        /// The try command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected override bool TryCommand(
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            TimeSpan timeout)
        {
            return this.EndTryCommand(this.BeginTryCommand(context, command, timeout, null, null));
        }

        /// <summary>
        /// The deserialize data.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <returns>
        /// The <see cref="IDictionary"/>.
        /// </returns>
        private static IDictionary<XName, InstanceValue> DeserializeData(string payload)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(payload);
            IDictionary<XName, InstanceValue> data = new Dictionary<XName, InstanceValue>();
            var netDataContractSerializer = new NetDataContractSerializer();
            var instances = xmlDocument.GetElementsByTagName("InstanceValue");
            foreach (XmlElement instanceElement in instances)
            {
                var keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
                var key = (XName)DeserializeObject(netDataContractSerializer, keyElement);
                var valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
                var value = DeserializeObject(netDataContractSerializer, valueElement);
                var instVal = new InstanceValue(value);
                data.Add(key, instVal);
            }

            return data;
        }

        /// <summary>
        /// The deserialize object.
        /// </summary>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private static object DeserializeObject(IFormatter serializer, XmlNode element)
        {
            object deserializedObject = null;
            var memoryStream = new MemoryStream();
            var xmlDictionaryWriter = XmlDictionaryWriter.CreateTextWriter(memoryStream);
            element.WriteContentTo(xmlDictionaryWriter);
            xmlDictionaryWriter.Flush();
            memoryStream.Position = 0;
            deserializedObject = serializer.Deserialize(memoryStream);
            return deserializedObject;
        }

        /// <summary>
        /// The serialize object.
        /// </summary>
        /// <param name="elementName">
        /// The element name.
        /// </param>
        /// <param name="objectToSerialize">
        /// The object to serialize.
        /// </param>
        /// <param name="xmlDocument">
        /// The xml document.
        /// </param>
        /// <returns>
        /// The <see cref="XmlElement"/>.
        /// </returns>
        private static XmlElement SerializeObject(string elementName, object objectToSerialize, XmlDocument xmlDocument)
        {
            var netDataContractSerializer = new NetDataContractSerializer();
            var newElement = xmlDocument.CreateElement(elementName);
            var memoryStream = new MemoryStream();
            netDataContractSerializer.Serialize(memoryStream, objectToSerialize);
            memoryStream.Position = 0;
            var rdr = new StreamReader(memoryStream);
            newElement.InnerXml = rdr.ReadToEnd();
            return newElement;
        }

        /// <summary>
        /// The save instance data.
        /// </summary>
        /// <param name="instanceData">
        /// The instance data.
        /// </param>
        private void SaveInstanceData(IEnumerable<KeyValuePair<XName, InstanceValue>> instanceData)
        {
            //// Change workflow instance id to request id to establish correlation.
            var workflowInstanceData = instanceData as IList<KeyValuePair<XName, InstanceValue>>
                ?? instanceData.ToList();
            foreach (var instanceInfo in workflowInstanceData)
            {
                if (instanceInfo.Key.LocalName != "Workflow")
                {
                    continue;
                }

                var properties = instanceInfo.Value.Value.GetType().GetProperties();
                foreach (var propertyInfo in properties.Where(propertyInfo => propertyInfo.Name == "WorkflowInstanceId"))
                {
                    propertyInfo.SetValue(instanceInfo.Value.Value, this.ownerInstanceId);
                }
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<InstanceValues/>");
            foreach (var element in workflowInstanceData)
            {
                var newInstance = xmlDocument.CreateElement("InstanceValue");
                var newKey = SerializeObject("key", element.Key, xmlDocument);
                newInstance.AppendChild(newKey);
                var newValue = SerializeObject("value", element.Value.Value, xmlDocument);
                newInstance.AppendChild(newValue);
                if (xmlDocument.DocumentElement != null)
                {
                    xmlDocument.DocumentElement.AppendChild(newInstance);
                }
            }

            var data = new InstanceData
            {
                InstanceKey = "WorkflowInstanceStoreData",
                RequestId = this.ownerInstanceId.ToString(),
                Payload = xmlDocument.InnerXml.SplitByLength(5000).ToList(),
                EntityTag = "*"
            };
            this.unstructuredStorageRepository.Update(data);
            this.unstructuredStorageRepository.Save();
            var bookmarks = (from element in workflowInstanceData
                             where element.Key.LocalName.CompareCaseInvariant("Bookmarks")
                             select element.Value.Value).SingleOrDefault() as IEnumerable<BookmarkInfo>;
            if (bookmarks != null && bookmarks.Any())
            {
                this.submitStateMessage(this.ownerInstanceId);
            }
        }

        #endregion
    }
}
