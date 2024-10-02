# Vita線上訂餐平台 - 後台店家管理系統

## 專案簡介
2024.02.16 ~ 2024.07.04 資展國際「智慧應用微軟C#工程師就業養成班」的團體期末專案，
結合前端(Html、CSS、javaScript)、資料庫(SQL server)
以及微軟C#、ASP .NET MVC後端架構設計以「健康餐訂餐平台」為主題的網站。

平台分為前台客戶訂餐網頁，和後台店家管理系統。本人在成果發表中負責設計及實作後台系統的部分頁面。

本文件為個人在發表成果發表之後，修正不完善之處，用另外一種UI介面重製原專案的網頁。
為了區分兩者，以下分別用「本專案」、「msit59-Vita 團體專案」兩個段落說明各自完成的功能以及頁面呈現。

原團隊完整的專案程式碼須進入 [msit59-Vita](https://github.com/ezMarshall/msit59-vita.git) 查看。


## 使用技術

- 設計工具：Figma、draw.io、Canva
- 前端技術：HTML、CSS、JavaScript、jQuery、Ajax、RESTful API
- 前端框架：Bootstrap 5、SASS/SCSS、Chart.js
- 後端技術：C#、ASP.NET
- 資料庫：SSMS、SQL Server、PowerBI
- 版本控制：GitHub、Git
- 撰寫程式：Visual Studio 2022、Visual Studio Code


## 本專案

### 完成功能 & 簡單頁面呈現

- **首頁 / 今天網路訂單**

點選下拉式箭頭，可檢視網路下單的訂單明細，也可執行以下訂單操作:
- 接單: 接單後訂單狀態改為「製作中」，完成後可點選「出餐」按鈕
- 退單: 退單後會在訂單狀態改成顯示「已退單」
- 出餐: 出餐後等待外送或客戶自取，成功交貨後可點選「完成」按鈕
- 完成

![650x430_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E9%A6%96%E9%A0%81.png)

- **店家營運資訊**
點選編輯icon可修改指定的店家資訊欄位

![680x385_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E5%BA%97%E5%AE%B6%E8%B3%87%E8%A8%8A.png)

- **商品增刪查改**
商品分類: 編輯/刪除/新增
商品管理: 編輯/新增/複製(基於舊商品資訊新增商品)/上下架(無法直接刪除商品，但可下架商品讓客戶無法看到)

![650x430_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E5%95%86%E5%93%81%E8%A8%AD%E5%AE%9A.png)

- **過往訂單紀錄**
可篩選指定「客戶名稱、訂單編號、商品與數量」、「客戶或店家評語」、「評分星級」的訂單明細。

若客戶有真對該筆訂單建立評論，可點「檢視」按鈕查閱。若店家尚未回覆，也可點擊「回覆」按鈕填寫回覆內容。

![750x440_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E6%AD%B7%E5%8F%B2%E8%A8%82%E5%96%AE.png)


## msit59-Vita 團體專案 

### 完成功能 & 簡單頁面呈現

- 管理員登入

- **當日訂單即時資訊和營收統計**

[首頁圖表 - 影片連結](https://youtu.be/uphOwXcPf-c)

今日訂單資訊、本週新進評論超連結透過LocalStorage可連進訂單管理、評論管理頁面並篩選特定資料。

基於PowerBI設計圖表，利用Chart.js在網頁上繪製圓環圖、橫條圖、折線圖，AnyChart繪製拼貼圖(mosaic plot)。


- **菜單管理：新增、修改、下架商品和商品類別**

[菜單管理 - 影片連結](https://youtu.be/Dk875pWmHYg)

ASP .NET Core 實作 CRUD 功能。

利用MVC架構搭配EF Core及LINQ進行資料搜尋與排序。

Bootstrap 5 互動視窗(Modal)配置畫面。

透過AJAX來更新彈鈕的上下架狀態、商品分類的設定。


- **店家資訊管理、暫停接單功能、響應式設計**

[店家資訊&其他功能 - 影片連結](https://youtu.be/iZWpT5HGqZ4)

店家資訊修改時，前端表單驗證。

當後台暫停接單按鈕被點擊，透過SignalR立刻刷新前台店家頁面的接單狀態。

透過Bootstrap 5搭配CSS3 Media Query執行響應式設計。


- 訂單管理：查看訂單詳情、出餐管理

- 評論管理：查看及回覆評論


### 詳細專案負責內容
功能設計：後台 > 首頁、登入頁面、菜單管理、店家資訊

程式撰寫：後台 > 首頁、菜單管理、店家資訊
- 首頁圖表設計與實作
- 商品／商品分類增刪查改
- 店家資訊修改、表單驗證

企劃書：創作理念發想、前言和市場分析彙整與撰寫
資料庫建置與彙整、生成資料表內容



## 附註

資料庫建置方式：執行SQL指令檔「sql for StoreManagementPlatform」

系統登入帳密：
Vita0042
V3WV1rfk0yYEUV
