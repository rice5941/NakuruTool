# Model層 設計

- `〇〇Domain`クラスがデータの実体を管理します。
- `〇〇Domain`内に、データ操作を行う`〇〇Store`、検証ルールを持つ`〇〇ValidationRule`、検証を実行する`Validator`を実装します。
- `◯◯`クラスをデータクラスとします。
- `namespace`は`〇〇`とし、フォルダ名も`〇〇`とします。
