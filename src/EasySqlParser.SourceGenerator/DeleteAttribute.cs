using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SourceGenerator
{
    // TODO: DOC
    /// <summary>
    /// Attribute for DELETE sql
    /// </summary>
    public class DeleteAttribute : NonQueryAttribute
    {
        /// <summary>
        /// VersionNoを更新条件に含めない
        /// VersionNo自体は設定された値で更新される
        /// </summary>
        public bool IgnoreVersion { get; set; } = false;

        /// <summary>
        /// EfCoreが想定しているRowVersion型など特殊なものではなく、longなど一般的な型を使って楽観排他を行う
        /// 更新件数が0件の場合は `DbUpdateConcurrencyException` をスローする
        /// </summary>
        public bool UseVersion { get; set; } = false;

        /// <summary>
        /// VersionNoを更新条件に含める
        /// 更新件数0件でも `DbUpdateConcurrencyException` をスローしない
        /// </summary>
        public bool SuppressDbUpdateConcurrencyException { get; set; } = false;
    }
}
