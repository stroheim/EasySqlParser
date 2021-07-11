namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    /// Behavior during INSERT or UPDATE.
    /// </summary>
    public enum QueryBehavior
    {
        /// <summary>
        /// Changes are not returned to the entity.
        /// </summary>
        None,
        /// <summary>
        /// Only the values ​​in the auto-numbered column are returned to the entity.
        /// </summary>
        IdentityOnly,
        /// <summary>
        /// All values ​​are returned to the entity.
        /// </summary>
        AllColumns,
        /// <summary>
        /// If there is an auto-numbered column,
        /// only its value is returned,
        /// otherwise all columns are returned to the entity.
        /// </summary>
        IdentityOrAllColumns
    }

}
