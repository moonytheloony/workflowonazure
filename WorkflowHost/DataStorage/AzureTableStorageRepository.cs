// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="AzureTableStorageRepository.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.DataStorage
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    using WorkflowHost.Entities;

    #endregion

    /// <summary>
    /// Class AzureTableStorageRepository. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="TElement">The type of the t element.</typeparam>
    public sealed class AzureTableStorageRepository<TElement>
        where TElement : class, new()
    {
        #region Fields

        /// <summary>
        /// The converter from table entity to entity.
        /// </summary>
        private readonly Func<DynamicTableEntity, TElement> convertToEntity;

        /// <summary>
        /// The converter from entity to table entity.
        /// </summary>
        private readonly Func<TElement, DynamicTableEntity> convertToTableEntity;

        #endregion

        #region Constructors and Destructors


        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{TElement}"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="connectionString">The connection string.</param>
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
        /// <param name="tableName">Name of the table.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="convertToTableEntity">The convert to table entity.</param>
        /// <param name="convertToEntity">The convert to entity.</param>
        public AzureTableStorageRepository(
            string tableName,
            string connectionString,
            Func<TElement, DynamicTableEntity> convertToTableEntity,
            Func<DynamicTableEntity, TElement> convertToEntity)
        {
            this.TableName = tableName;
            this.convertToTableEntity = convertToTableEntity;
            this.convertToEntity = convertToEntity;

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            this.CloudTableClient = storageAccount.CreateCloudTableClient();
            this.CloudTableClient.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 5);
            this.TableRequestOptions = new TableRequestOptions
                                           {
                                               RetryPolicy =
                                                   new ExponentialRetry(TimeSpan.FromSeconds(2), 5)
                                           };
            this.TableOperations = new TableBatchOperation();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the active table.
        /// </summary>
        /// <value>The active table.</value>
        private CloudTable ActiveTable { get; set; }

        /// <summary>
        /// Gets or sets the cloud table client.
        /// </summary>
        /// <value>The cloud table client.</value>
        private CloudTableClient CloudTableClient { get; }

        /// <summary>
        /// Gets or sets the table operation.
        /// </summary>
        /// <value>The table operations.</value>
        private TableBatchOperation TableOperations { get; set; }

        /// <summary>
        /// Gets or sets the table request options.
        /// </summary>
        /// <value>The table request options.</value>
        private TableRequestOptions TableRequestOptions { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create storage object and set execution context.
        /// </summary>
        public void CreateStorageObjectAndSetExecutionContext()
        {
            this.ActiveTable = this.CloudTableClient.GetTableReference(this.TableName);
            this.ActiveTable.CreateIfNotExists(this.TableRequestOptions);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Delete(TElement entity)
        {
            var dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.Delete(dynamicEntity));
        }

        /// <summary>
        /// The delete storage object.
        /// </summary>
        public void DeleteStorageObject()
        {
            this.ActiveTable.DeleteIfExists(this.TableRequestOptions);
        }

        /// <summary>
        /// The get all.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="List{T}" />.</returns>
        public IList<TElement> GetAll(string key)
        {
            var query =
                new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
            var result = this.ActiveTable.ExecuteQuery(query, this.TableRequestOptions);
            return result.Select(this.convertToEntity).ToList();
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>IList&lt;TElement&gt;.</returns>
        /// <inheritdoc />
        public IList<TElement> GetAll()
        {
            var result = this.ActiveTable.ExecuteQuery(new TableQuery(), this.TableRequestOptions);
            return result.Select(this.convertToEntity).ToList();
        }

        /// <summary>
        /// The get by id.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="TElement" />.
        /// Element for entity</returns>
        public TElement GetById(string key, string id)
        {
            var operation = TableOperation.Retrieve(key, id);
            var result = this.ActiveTable.Execute(operation, this.TableRequestOptions).Result as DynamicTableEntity;
            return result == null ? null : this.convertToEntity(result);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Insert(TElement entity)
        {
            var dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.Insert(dynamicEntity));
        }

        /// <summary>
        /// Inserts the or replace.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <inheritdoc />
        public void InsertOrReplace(TElement entity)
        {
            var dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrReplace(dynamicEntity));
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Merge(TElement entity)
        {
            var dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrMerge(dynamicEntity));
        }

        /// <summary>
        /// Queries the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="takeCount">The take count.</param>
        /// <returns>IList&lt;TElement&gt;.</returns>
        /// <inheritdoc />
        public IList<TElement> Query(string filter, int? takeCount)
        {
            var tableQuery = new TableQuery { FilterString = filter, TakeCount = takeCount };
            return this.ActiveTable.ExecuteQuery(tableQuery).Select(this.convertToEntity).ToList();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <returns>The <see cref="bool" />.</returns>
        public bool Save()
        {
            try
            {
                var result = this.ActiveTable.Execute(this.TableOperations[0], this.TableRequestOptions);
                this.TableOperations = new TableBatchOperation();
                return IsSuccessStatusCode(result.HttpStatusCode);
            }
            catch (StorageException exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves all.
        /// </summary>
        /// <returns>IList&lt;OperationResult&gt;.</returns>
        /// <inheritdoc />
        public IList<OperationResult> SaveAll()
        {
            try
            {
                var result = this.ActiveTable.ExecuteBatch(this.TableOperations, this.TableRequestOptions);
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
        /// The set execution context.
        /// </summary>
        public void SetExecutionContext()
        {
            this.ActiveTable = this.CloudTableClient.GetTableReference(this.TableName);
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Update(TElement entity)
        {
            var dynamicEntity = this.convertToTableEntity(entity);
            this.TableOperations.Add(TableOperation.InsertOrReplace(dynamicEntity));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the HTTP status code represents a success.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>If the status code represents a success.</returns>
        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        #endregion
    }
}