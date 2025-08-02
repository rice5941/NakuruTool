# View層 設計

- UI要素を`MainWindow.xaml`に直接記述せず、機能単位で`UserControl`に分割します。
- 複数のViewで利用するConverterは`CommonConverter`クラスに集約します。

## コード規約

- **コードビハインドの記述範囲**: Viewのコードビハインド（`.xaml.cs`）には、UIに直接関わる処理以外は実装しないでください。
- **デザイン時DataContextの設定**: XAMLエディタのIntelliSenseを有効にするため、各Viewにはデザイン時DataContext (`d:DataContext`) を設定してください。