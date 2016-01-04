using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.DataStorage
{
    using System.Diagnostics.Contracts;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    using WorkflowHost.Entities;

    public class AzureTableStorageRepository<TElement>
        where TElement : class, new()
    {
        #region Fields

        /// <summary>
        ///     The converter from table entity to entity.
        /// </summary>
        private readonly Func<DynamicTableEntity, TElement> convertToEntity;

        /// <summary>
        ///     The converter from entity to table entity.
        /// </summary>
        private readonly Func<TElement, DynamicTableEntity> convertToTableEntity;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{TElement}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public AzureTableStorageRepository(string tableName, string connectionString)
            : this(
                tableName,
                connectionString,
                AzureTableStorageAssist.ConvertEntityToDynamicTableEntity,
                AzureTableStorageAssist.ConvertDynamicEntityToEntity<TElement>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{TElement}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="convertToTableEntity">
        /// The converter from entity to table entity.
        /// </param>
        /// <param name="convertToEntity">
        /// The converter from table entity to entity.
        /// </param>
        public AzureTableStorageRepository(
            string tableName, string connectionString,
           Func<TElement, DynamicTableEntity> convertToTableEntity,
            Func<DynamicTableEntity, TElement> convertToEntity)
        {
            Contract.Requires<Exception>(null != convertToTableEntity, "convertToTableEntity");
            Contract.Requires<Exception>(null != convertToEntity, "convertToEntity");

            this.TableName = tableName;
            this.convertToTableEntity = convertToTableEntity;
            this.convertToEntity = convertToEntity;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            this.CloudTableClient = storageAccount.CreateCloudTableClient();
            this.CloudTableClient.DefaultRequestOptions.RetryPolicy =
                new ExponentialRetry(
                    TimeSpan.FromSeconds(2),
                    5);
            this.TableRequestOptions = new TableRequestOptions
            {
                RetryPolicy =
                        new ExponentialRetry(
                            TimeSpan.FromSeconds(2),
                            5)
            };
            this.TableOperations = new TableBatchOperation();
        }

        public string TableName { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the active table.
        /// </summary>
        private CloudTable ActiveTable { get; set; }

        /// <summary>
        ///     Gets or sets the cloud table client.
        /// </summary>
        private CloudTableClient CloudTableClient { get; set; }

        /// <summary>
        ///     Gets or sets the table operation.
        /// </summary>
        private TableBatchOperation TableOperations { get; set; }

        /// <summary>
        ///     Gets or sets the table request options.
        /// </summary>
        private TableRequestOptions TableRequestOptions { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The create storage object and set execution context.
        /// </summary>
        public virtual void CreateStorageObjectAndSetExecutionContext()
        {
            this.ActiveTable = this.CloudTableClient.GetTableReference(this.TableName);
            this.ActiveTable.CreateIfNotExists(this.TableRequestOptions);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        public virtual void Delete(TElement entity)
        {
            DynamicTableEntity dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.Delete(dynamicEntity));
        }

        /// <summary>
        ///     The delete storage object.
        /// </summary>
        public void DeleteStorageObject()
        {
            this.ActiveTable.DeleteIfExists(this.TableRequestOptions);
        }

        /// <summary>
        /// The get all.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        public virtual IList<TElement> GetAll(string key)
        {
            TableQuery query =
                new TableQuery().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
            IEnumerable<DynamicTableEntity> result = this.ActiveTable.ExecuteQuery(query, this.TableRequestOptions);
            return result.Select(this.convertToEntity).ToList();
        }

        /// <inheritdoc />
        public virtual IList<TElement> GetAll()
        {
            IEnumerable<DynamicTableEntity> result = this.ActiveTable.ExecuteQuery(
                new TableQuery(),
                this.TableRequestOptions);
            return result.Select(this.convertToEntity).ToList();
        }

        /// <summary>
        /// The get by id.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="TElement"/>.
        ///     Element for entity
        /// </returns>
        public virtual TElement GetById(string key, string id)
        {
            TableOperation operation = TableOperation.Retrieve(key, id);
            var result = this.ActiveTable.Execute(operation, this.TableRequestOptions).Result as DynamicTableEntity;
            return result == null ? null : this.convertToEntity(result);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        public virtual void Insert(TElement entity)
        {
            DynamicTableEntity dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.Insert(dynamicEntity));
        }

        /// <inheritdoc />
        public virtual void InsertOrReplace(TElement entity)
        {
            DynamicTableEntity dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrReplace(dynamicEntity));
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        public virtual void Merge(TElement entity)
        {
            DynamicTableEntity dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrMerge(dynamicEntity));
        }

        /// <inheritdoc />
        public virtual IList<TElement> Query(string filter, int? takeCount)
        {
            var tableQuery = new TableQuery { FilterString = filter, TakeCount = takeCount };
            return this.ActiveTable.ExecuteQuery(tableQuery).Select(this.convertToEntity).ToList();
        }

        /// <summary>
        ///     The save.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public virtual bool Save()
        {
            try
            {
                TableResult result = this.ActiveTable.Execute(this.TableOperations[0], this.TableRequestOptions);
                this.TableOperations = new TableBatchOperation();
                return IsSuccessStatusCode(result.HttpStatusCode);
            }
            catch (StorageException exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public virtual IList<OperationResult> SaveAll()
        {
            try
            {
                IList<TableResult> result = this.ActiveTable.ExecuteBatch(
                    this.TableOperations,
                    this.TableRequestOptions);
                this.TableOperations = new TableBatchOperation();
                return
                    result.Select(x => new OperationResult(x.HttpStatusCode, IsSuccessStatusCode(x.HttpStatusCode)))
                        .ToList();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     The set execution context.
        /// </summary>
        public virtual void SetExecutionContext()
        {
            this.ActiveTable = this.CloudTableClient.GetTableReference(this.TableName);
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        public virtual void Update(TElement entity)
        {
            DynamicTableEntity dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrReplace(dynamicEntity));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the HTTP status code represents a success.
        /// </summary>
        /// <param name="statusCode">
        /// The status code.
        /// </param>
        /// <returns>
        /// If the status code represents a success.
        /// </returns>
        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        #endregion
    }
}
