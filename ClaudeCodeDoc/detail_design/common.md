# 共通機能設計

- **プロパティ変更通知**: ViewModelの基底クラスに、プロパティの値を設定しつつ`PropertyChanged`イベントを発行する共通の`SetProperty`関数を実装し、各ViewModelはこれを必ず使用します。

# 全体設計

- **基本構成**: .net6.0, WPF, Livet を使用したMVVMパターンを採用します。
- **アーキテクチャ**: **View, ViewModel, Feature, Model** の4層に加え、通信処理を分離する **Adapter** 層を設けます。
- **レイヤー間のルール**:
    - Model層とViewModel層は、互いを直接参照しません。
    - ViewModel層とModel層の連携は、必ずFeature層を介して行います。