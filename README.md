# Social Network Analyzer

## Shrnutí zadání: 
Vytvoření webové aplikace pro generování statistik vztahů mezi uživateli imaginární sociální sítě ze souboru.

## Postup rěšení:
Vzhledem k tomu že jsem Abp Framework používal poprvé a s webovými aplikacemi jsem taky neměl předchozí velké zkušenosti a vzhledem k omezenému času jsem se rozhodl pro co nejpřímočarejší řešení abych měl jistotu že toho stihnu co nejvíc (ideálně všechcno).
Zvolil jsem proto single-layer template pro začátek projektu (i vzhledem k relativné jednoduchosti celé aplikace).

### Backend:
Začal jsem návrhem entit. Založil jsem 2 entity:
1. Dataset - reprezentuje datový set jako celek - obsahuje jméno, vypočtené statistiky a zahashované data (kvůli kontrole duplicity datasetu)
1. DatasetItem - jeden záznam reprezentuje jeden řádek v datovém setu - obsahuje ještě název Datasetu 

Následně jsem začal řešit jak dostat datový set do DB a jak ho zobrazit na frontendu.
Pro (skoro) všechny backend metody byl vytvořen ApplicationService DatasetAppService

Na získaní seznamu datasetů po načtení aplikace slouží metoda GetListAsync.

Načtení do DB byl pro mě trochu problém - snažil jsem se odeslat soubor přes frontend do backend metody PopulateDatasetAsync přes Dynamic JavaScript Client Proxies. 
Poměrne dlouho mi trvalo zjistit jak soubor správně poslat aby jsem nedostával rúzné nicneříkajíci errory. 

Následně jsem řešil nahrávání duplicitního datasetu pomocí zahashování všech dat a porovnání s DB a další výjimky které můžou nastat.

Další větší problém nastal u samotného nahrávaní položek datasetu do DB kvůli velikosti datasetu (85k řádků). 
Nahrávaní trvalo desítky minut. Nakonec jsem vytvořil nový UnitOfWork scope kolem samotného sypání dat do DB a data rozdělil do dávek, tím se zkrátilo nahrávaní vzorového datasetu na cca 2minuty.

### Frontend:
Po načtení aplikace se načte a zobrazí seznam Datasetů.

Uživatel může poklikem vybrat Dataset a otevřou se statistiky pro konkrétní Dataset.

Načítání statistik probíhá dynamicky pomocí JavaScript metody openModal, která zavolá backendovou metodu pro nalezení datasetu podle jména a vloží tabulku s daty z DB do modálního okna.

Při nahrávání nového datasetu se soubor s daty odesílá do backendové metody, která vrátí objekt Dto Datasetu. Tento objekt je poté použit k přidání nového datasetu do seznamu na stránce.
Z důvodu někdy delšího nahrávání datasetu do DB jsem přidal hlášky které informují uživatele o stavu zpracování nebo případné chybě.

### Unit testy:
Bohužel jsem nestihl implementovat všechny případy a už jsem to odevzdání nechtěl dál prodlužovat, takže je to spíš taková vzorka.

Automatické vytvoření DB po startu aplikace se mi nepovedlo udělat, takže DB se musí vytvořit pomocí 'dotnet ef database update' příkazu.
