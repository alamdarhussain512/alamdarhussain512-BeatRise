Added Underground market system: UndergroundOffer model, UndergroundMarketManager, MarketUIManager runtime UI, and balancing tuning. These are fictional in-game mechanics only.

Usage notes:
- Add UndergroundMarketManager and MarketUIManager to the SocialHub scene (empty GameObjects) to enable the market UI and actions.
- Offers cost in-game money and may increase notoriety; detection can cause fines and reputation loss. All effects are saved via GameManager.Save().
