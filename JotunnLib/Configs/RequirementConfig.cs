﻿using Jotunn.Entities;

namespace Jotunn.Configs
{
    public class RequirementConfig
    {
        public string Item { get; set; }
        public int Amount { get; set; }
        public int AmountPerLevel { get; set; }
        public bool Recover { get; set; }

        public Piece.Requirement GetRequirement()
        {
            return new Piece.Requirement()
            {
                m_resItem = Mock<ItemDrop>.Create(Item),
                m_amount = Amount,
                m_amountPerLevel = AmountPerLevel,
                m_recover = Recover
            };
        }
    }
}
