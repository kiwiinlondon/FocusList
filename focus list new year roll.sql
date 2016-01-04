-----------------------------
-- Run each step on first day back at work in new year
-- Change the dates obvs
-----------------------------

--backup just in case
SELECT * INTO FocusList2016NewYearBackup FROM FocusList


--set prices to end of year
UPDATE
    fl
SET
    fl.CurrentPrice = p.Value,
    fl.CurrentPriceDate = p.ReferenceDate,
	fl.CurrentPriceId = p.PriceId,
	fl.RelativeCurrentPrice = pRel.Value,
	fl.RelativeCurrentPriceDate = pRel.ReferenceDate,
	fl.RelativeCurrentPriceId = pRel.PriceId
FROM
    FocusList fl
INNER JOIN
    Price p ON p.InstrumentMarketId = fl.InstrumentMarketId AND p.ReferenceDate='31-Dec-2015'
INNER JOIN
    Price pRel ON pRel.InstrumentMarketId = fl.RelativeIndexInstrumentMarketId AND pRel.ReferenceDate='31-Dec-2015'
WHERE
    p.Value IS NOT NULL
	AND fl.OutDate IS NULL


--check
SELECT dim.InstrumentName, dim.InstrumentMarketId, fl.CurrentPriceDate, fl.CurrentPrice, p1.Value AS Price2015, p2.Value AS PriceNow, fl.RelativeCurrentPrice, pRel1.Value AS Rel2015, pRel2.Value AS RelNow,
fl.* FROM dbo.FocusList fl
LEFT JOIN dbo.DenormalisedInstrumentMarket dim ON dim.InstrumentMarketId = fl.InstrumentMarketId
LEFT JOIN Price p1 ON p1.InstrumentMarketId = fl.InstrumentMarketId AND p1.ReferenceDate='31-Dec-2015'
LEFT JOIN Price p2 ON p2.InstrumentMarketId = fl.InstrumentMarketId AND p2.ReferenceDate='04-Jan-2016'
LEFT JOIN Price pRel1 ON pRel1.InstrumentMarketId = fl.RelativeIndexInstrumentMarketId AND pRel1.ReferenceDate='31-Dec-2015'
LEFT JOIN Price pRel2 ON pRel2.InstrumentMarketId = fl.RelativeIndexInstrumentMarketId AND pRel2.ReferenceDate='04-Jan-2016'
WHERE fl.OutDate IS NULL
ORDER BY dim.InstrumentName

SELECT * FROM dbo.Price WHERE InstrumentMarketId=20105 ORDER BY ReferenceDate desc

-- snapshot table with corrected prices
SELECT * INTO FocusList2015 FROM FocusList

----------------
-- Now run focus lists SSRS and export to excel.
-- first however need to temporarily release Reporting and set beginning of year dates to last year
-- change dates in GetSinceYearEnd and GetShortSinceYearEnd in Odey.Reporting.Web.Internal.FocusList
---------------


--restore prices to now. 
--Also set correct end of year prices to end of last year
UPDATE
    fl
SET
    fl.CurrentPrice = p.Value,
    fl.CurrentPriceDate = p.ReferenceDate,
	fl.CurrentPriceId = p.PriceId,
	fl.RelativeCurrentPrice = pRel.Value,
	fl.RelativeCurrentPriceDate = pRel.ReferenceDate,
	fl.RelativeCurrentPriceId = pRel.PriceId,
	fl.EndOfYearPrice = pEnd.Value,
	fl.RelativeEndOfYearPrice = pRelEnd.Value
FROM
    FocusList fl
INNER JOIN
    Price p ON p.InstrumentMarketId = fl.InstrumentMarketId AND p.ReferenceDate='04-jan-2016'
INNER JOIN
    Price pRel ON pRel.InstrumentMarketId = fl.RelativeIndexInstrumentMarketId AND pRel.ReferenceDate='04-jan-2016'
INNER JOIN
    Price pEnd ON pEnd.InstrumentMarketId = fl.InstrumentMarketId AND pEnd.ReferenceDate='31-Dec-2015'
INNER JOIN
    Price pRelEnd ON pRelEnd.InstrumentMarketId = fl.RelativeIndexInstrumentMarketId AND pRelEnd.ReferenceDate='31-Dec-2015'
WHERE
    p.Value IS NOT NULL
	AND fl.OutDate IS NULL

----------------
-- restore dates in GetSinceYearEnd and GetShortSinceYearEnd in Odey.Reporting.Web.Internal.FocusList
---------------

--drop temp table
DROP TABLE FocusList2016NewYearBackup
