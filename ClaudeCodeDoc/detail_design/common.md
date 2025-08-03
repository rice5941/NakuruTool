# 共通機能設計

- **プロパティ変更通知**: ViewModelの基底クラスに、プロパティの値を設定しつつ`PropertyChanged`イベントを発行する共通の`SetProperty`関数を実装し、各ViewModelはこれを必ず使用します。

# 全体設計

- **基本構成**: .net6.0, WPF, Livet を使用したMVVMパターンを採用します。