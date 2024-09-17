# Vita線上訂餐平台 - 後台店家管理系統


### 專案簡介
2024.02.16 ~ 2024.07.04 資展國際「智慧應用微軟C#工程師就業養成班」的團體期末專案，結合前端(Html、CSS、javaScript)、資料庫(SQL server)以及微軟C#、ASP .NET MVC後端架構設計以「健康餐訂餐平台」為主題的網站。

這邊只呈現本人實作頁面的說明，完整的專案程式碼須進入 [msit59-Vita](https://github.com/ezMarshall/msit59-vita.git) 查看。

本人實作的頁面主要在店家後台的管理系統，店家可藉由【菜單管理】設定可供訂餐的品項、【店家資訊】設定營業時間、電話等基本資訊。首頁的統計圖表可檢視近一週客人對店家商品的評論、近一個月的營運表現（諸如各商品銷售額比較、各營業日來客量變化）。暫停接單功能可供店家即時終止前台客戶下單。

### 後臺完成功能

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


### 使用技術

- 設計工具：Figma、draw.io、Canva
- 前端技術：HTML、CSS、JavaScript、jQuery、Ajax、RESTful API
- 前端框架：Bootstrap 5、SASS/SCSS、Chart.js
- 後端技術：C#、ASP.NET
- 資料庫：SSMS、SQL Server、PowerBI
- 版本控制：GitHub、Git
- 撰寫程式：Visual Studio 2022、Visual Studio Code


### 詳細專案負責內容
功能設計：後台 > 首頁、登入頁面、菜單管理、店家資訊

程式撰寫：後台 > 首頁、菜單管理、店家資訊
- 首頁圖表設計與實作
- 商品／商品分類增刪查改
- 店家資訊修改、表單驗證

企劃書：創作理念發想、前言和市場分析彙整與撰寫
資料庫建置與彙整、生成資料表內容

### 簡單頁面呈現

- 首頁功能

![500x250_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E5%8D%B3%E6%99%82%E8%A8%82%E5%96%AE%E8%B3%87%E8%A8%8A.png)

![500x330_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E5%89%8D%E9%80%B1%E8%A9%95%E8%AB%96%E5%88%86%E6%9E%90.png)

![700x248_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E7%B5%B1%E8%A8%88%E5%9C%96%E8%A1%A8.png)


- 菜單管理

![650x350_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E8%8F%9C%E5%96%AE%E7%AE%A1%E7%90%86%E9%A0%81%E9%9D%A2.png)


- 店家資訊 & 網頁前端配置

![640x336_default (1)](https://github.com/simonwu513/VitaStoreManagementPlatform/blob/main/%E5%BA%97%E5%AE%B6%E8%B3%87%E8%A8%8A%E9%A0%81%E9%9D%A2%E8%88%87%E7%B6%B2%E9%A0%81%E9%85%8D%E7%BD%AE.png)


## 附註
PowerPoint投影片放映可呈現各頁面GIF動圖。
