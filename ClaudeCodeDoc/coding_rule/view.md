## コード規約

- **コードビハインドの記述範囲**: Viewのコードビハインド（`.xaml.cs`）には、UIに直接関わる処理以外は実装しないでください。
- **デザイン時DataContextの設定**: XAMLエディタのIntelliSenseを有効にするため、各Viewにはデザイン時DataContext (`d:DataContext`) を設定してください。