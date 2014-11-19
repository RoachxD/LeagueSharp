--[[
                Sida's Auto Reborn
]]
 
local AutoCarryGlobal = {}
AutoCarryGlobal.Data = {}
_G.AutoCarry = AutoCarryGlobal.Data
 
local DoneInit = false
function OnTick()
 
if not DoneInit then
        if not Init() then
                return
        end
end
 
        if Keys.LastHit then
                Minions:LastHit()
        end
 
        if Keys.AutoCarry then
                Items:UseAll(Crosshair.Attack_Crosshair.target)
                Skills:CastAll(Crosshair:GetTarget())
                Orbwalker:Orbwalk(Crosshair.Attack_Crosshair.target)
        end
 
        if Keys.LaneClear then
                Skills:CastAll(Crosshair:GetTarget())
                if LaneClearMenu.MinionPriority and Orbwalker:CanOrbwalkTarget(Minions.KillableMinion) then
                        Orbwalker:Orbwalk(Minions.KillableMinion)
                elseif Orbwalker:CanOrbwalkTarget(Crosshair.Attack_Crosshair.target) then
                        Items:UseAll(Crosshair.Attack_Crosshair.target)
                        Orbwalker:Orbwalk(Crosshair.Attack_Crosshair.target)
                elseif Orbwalker:CanOrbwalkTarget(Jungle:GetAttackableMonster()) then
                        Orbwalker:Orbwalk(Jungle:GetAttackableMonster())
                else
                        Minions:LaneClear()
                end
        end
 
        if Keys.MixedMode then         
                Skills:CastAll(Crosshair:GetTarget())
                if MixedModeMenu.MinionPriority and Orbwalker:CanOrbwalkTarget(Minions.KillableMinion) then
                        Orbwalker:Orbwalk(Minions.KillableMinion)
                elseif Orbwalker:CanOrbwalkTarget(Crosshair.Attack_Crosshair.target) then
                        Items:UseAll(Crosshair.Attack_Crosshair.target)
                        Orbwalker:Orbwalk(Crosshair.Attack_Crosshair.target)
                else
                        Minions:LastHit()
                end
        end
end
 
AdvancedCallback:bind('OnLoseVision', function(unit) end)
AdvancedCallback:bind('OnGainVision', function(unit) end)
AdvancedCallback:bind('OnDash', function(unit) end)
AdvancedCallback:bind('OnGainBuff', function(unit, buff) end)
AdvancedCallback:bind('OnLoseBuff', function(unit, buff) end)
AdvancedCallback:bind('OnUpdateBuff', function(unit, buff) end)
 
 
MODE_AUTOCARRY = 0
MODE_MIXEDMODE = 1
MODE_LASTHIT = 2
MODE_LANECLEAR = 3
AutoCarry.MODE_AUTOCARRY = 0
AutoCarry.MODE_MIXEDMODE = 1
AutoCarry.MODE_LASTHIT = 2
AutoCarry.MODE_LANECLEAR = 3
 
--[[
                Orbwalker Class
                                                ]]
class '_Orbwalker' Orbwalker = nil
 
STAGE_SHOOT = 0
STAGE_MOVE = 1
STAGE_SHOOTING = 2
RegisteredOnAttacked = {}
 
function _Orbwalker:__init()
        self.LastAttack = 0
        self.LastWindUp = 0
        self.AttackCooldown = 0
        self.AttackCompletesAt = 0
        self.AttackBufferMin = 50
        self.AttackBufferMax = 400
        self.AfterAttackTime = 0
        self.FleeingEnemyRangeBuffer = 0
        self.Stage = 0
        self.LastEnemyAttacked = nil
        Orbwalker = self
 
        AddTickCallback(function() self:_OnTick() end)
        AddProcessSpellCallback(function(Unit, Spell) self:_OnProcessSpell(Unit, Spell) end)
        AddAnimationCallback(function(Unit, Animation) self:_OnAnimation(Unit, Animation) end)
end
 
local lastTick = 0
function _Orbwalker:_OnTick()
        --self.Boost = ConfigMenu.Boost - (myHero.attackSpeed * 15) - (Helper.Latency * 0.5)
        --self.AttackBoost = ConfigMenu.AttackBoost
        lastTick = GetTickCount()
        if Helper.Tick + Helper.Latency / 2 > self.LastAttack + self.AttackCooldown then
                self.Stage = STAGE_SHOOT
        elseif Helper.Tick + Helper.Latency / 2 > self.LastAttack + self.LastWindUp + self.AttackBufferMin + (Data.ChampionData[myHero.charName] and Data.ChampionData[myHero.charName].BugDelay or 0) then
                self.Stage = STAGE_MOVE
        end
 
        if self.Stage == STAGE_MOVE and Helper.Tick  + Helper.Latency < self.LastAttack + self.LastWindUp + self.AttackBufferMax then
                self.AfterAttackTime = self.LastAttack + self.LastWindUp + self.AttackBufferMax
                Orbwalker:_OnAttacked()
        end
end
 
function _Orbwalker:_OnProcessSpell(Unit, Spell)
        if Unit.isMe then
                if Data:IsAttack(Spell) then
                        self.LastAttack = Helper.Tick
                        self.LastWindUp = Spell.windUpTime * 1000
                        self.AttackCooldown = Spell.animationTime * 1000
                        self.AttackCompletesAt = self.LastAttack + self.LastWindUp
                        self.LastAttackSpeed = myHero.attackSpeed
                        self.Stage = STAGE_SHOOTING
                        self.SlowChecked = false
                elseif Data:IsResetSpell(Spell) then
                        self:ResetAttackTimer()
                end
        elseif Spell.target and Spell.name == "SummonerExhaust" and Spell.target.isMe then
                self.AttackCooldown = self.AttackCooldown * 1.5
        elseif Spell.target and Spell.name == "Wither" and Spell.target.isMe then
                self.AttackCooldown = self.AttackCooldown * 1.35
        end
end
 
function _Orbwalker:_OnAttacked()
        for _, func in pairs(RegisteredOnAttacked) do
                func()
        end
end
 
function _Orbwalker:_OnAnimation(Unit, Animation)
        if Helper.Tick < Orbwalker.AttackCompletesAt and Unit.isMe and (Animation == "Run" or Animation == "Idle") then
                Orbwalker:ResetAttackTimer()
        end
end
 
function _Orbwalker:Orbwalk(target)
        if target and self.Stage == STAGE_SHOOT and self:CanOrbwalkTarget(target) then
                MyHero:Attack(target)
        elseif self.Stage ~= STAGE_SHOOTING then
                MyHero:Move()
        end
end
 
function _Orbwalker:OrbwalkIgnoreChecks(target)
        if target and self.Stage == STAGE_SHOOT then
                MyHero:Attack(target)
        elseif self.Stage == STAGE_MOVE then
                MyHero:Move()
        end
end
 
function _Orbwalker:ResetAttackTimer()
        self.LastAttack = Helper.Tick - Helper.Latency / 2 - self.AttackCooldown
end
 
function _Orbwalker:GetNextAttackTime()
        return self.LastAttack + self.AttackCooldown - Helper.Latency / 2
end
 
function _Orbwalker:IsAfterAttack()
        if self then
                return Helper.Tick + Helper.Latency / 2 < self.AfterAttackTime
        end
end
 
function _Orbwalker:CanOrbwalkTarget(Target)
        if ValidTarget(Target) then
                if Target.type == myHero.type then
                        local predPos = AutoCarry.GetPrediction({range=MyHero.TrueRange, speed=math.huge, delay=Orbwalker.LastWindUp}, Target)
                        if predPos and GetDistance(predPos) - Data:GetGameplayCollisionRadius(Target.charName) + self.FleeingEnemyRangeBuffer > MyHero.TrueRange then
                                return false
                        end
                end
                local CheckRange
                if MyHero.IsMelee and Target.type ~= myHero.type then
                        CheckRange = MyHero.TrueRange + 75
                else
                        CheckRange = MyHero.TrueRange
                end
                return GetDistance(Target) - Data:GetGameplayCollisionRadius(Target.charName) < CheckRange
        end
        return false
end
 
function _Orbwalker:IsShooting()
        return self.Stage == STAGE_SHOOTING
end
 
function RegisterOnAttacked(func)
        table.insert(RegisteredOnAttacked, func)
end
 
--[[                                   
                _MyHero Class
                                                ]]
 
class '_MyHero' MyHero = nil
 
function _MyHero:__init()
        self.Range = myHero.range
        self.HitBox = GetDistance(myHero.minBBox)
        self.GameplayCollisionRadius = Data:GetGameplayCollisionRadius(myHero.charName)
        self.TrueRange = self.Range + self.GameplayCollisionRadius
        self.IsMelee = myHero.range < 300
        self.MoveDistance = 480
        self.LastHitDamageBuffer = -10 --TODO
        self.ChampionAdditionalLastHitDamage = 0
        self.ItemAdditionalLastHitDamage = 0
        self.MasteryAdditionalLastHitDamage = 0
        self.Team = myHero.team == 100 and "Blue" or "Red"
        self.ProjectileSpeed = Data:GetProjectileSpeed(myHero.charName)
        self.LastMoved = 0
        self.MoveDelay = 50
        self.CanMove = true
        self.CanAttack = true
        self.InStandZone = false
        self.HasStopped = false
        MyHero = self
 
        AddTickCallback(function() self:_OnTick() end)
end
 
function _MyHero:_OnTick()
        if myHero.range ~= self.Range then
                if myHero.range and myHero.range > 0 and myHero.range < 1500 then
                        self.Range = myHero.range
                        self.TrueRange = self.Range + self.GameplayCollisionRadius - 10
                        self.IsMelee = myHero.range < 300
                end
        end
        if GetDistance(mousePos) < ExtrasMenu["StandZone"..myHero.charName] then
                self.InStandZone = true
        else
                self.InStandZone = false
        end
        self:CheckStopMovement()
end
 
function _MyHero:GetTimeToHitTarget(Target)
        local AttackTime = (GetDistance(Target) / self.ProjectileSpeed) + (Helper.Latency / 2)
        local NextAttack = Helper.Tick
        return AttackTime + NextAttack
end
 
function _MyHero:GetTotalAttackDamageAgainstTarget(Target, LastHit)
        local MyDamage = myHero:CalcDamage(Target, myHero.totalDamage)
        if LastHit then
                MyDamage = MyDamage + self.ChampionAdditionalLastHitDamage -- TODO
                MyDamage = MyDamage + self.ItemAdditionalLastHitDamage
                MyDamage = MyDamage + self:GetMasteryAdditionalLastHitDamage(MyDamage, Target)
                MyDamage = MyDamage + self.LastHitDamageBuffer
        end
        return MyDamage
end
 
function _MyHero:GetMasteryAdditionalLastHitDamage(Damage, Target)
        local _Damage = Damage
        _Damage = _Damage + Items:GetBotrkBonusLastHitDamage(_Damage, Target)
        _Damage = ConfigMenu.Spellblade and _Damage + (myHero.ap * 0.05) or _Damage
        _Damage = (ConfigMenu.Executioner and Target.health / Target.maxHealth < 0.5) and _Damage + (_Damage * 0.05) or _Damage
        _Damage = _Damage + (ConfigMenu.Butcher * 2)
        return _Damage - Damage - 1
end
 
function _MyHero:Move()
        if self:HeroCanMove() and not Helper:IsEvading() then
                if Helper.Tick > self.LastMoved + self.MoveDelay then
                        Streaming:OnMove()
                        local Distance = self.MoveDistance + Helper.Latency / 10
                        if self.IsMelee and Crosshair.Target and Crosshair.Target.type == myHero.type and GetDistance(Crosshair.Target) < 80 then
                                return
                        elseif GetDistance(mousePos) < Distance and GetDistance(mousePos) > 100 then
                                Distance = GetDistance(mousePos)
                        end
 
                        local MoveSqr = math.sqrt((mousePos.x - myHero.x) ^ 2 + (mousePos.z - myHero.z) ^ 2)
                        local MoveX = myHero.x + Distance * ((mousePos.x - myHero.x) / MoveSqr)
                        local MoveZ = myHero.z + Distance * ((mousePos.z - myHero.z) / MoveSqr)
 
                        myHero:MoveTo(MoveX, MoveZ)
                        self.LastMoved = Helper.Tick
                        self.HasStopped = false
                end
        end
end
 
function _MyHero:Attack(target)
        if self.CanAttack and not Helper:IsEvading() then
                if target.type ~= myHero.type then
                        MuramanaOff()
                end
 
                for _, func in pairs(Plugins.RegisteredPreAttack) do
                        func(target)
                end
 
                myHero:Attack(target)
                Orbwalker.LastEnemyAttacked = target
        end
end
 
function _MyHero:MovementEnabled(canMove)
        self.CanMove = canMove
end
 
function _MyHero:AttacksEnabled(canAttack)
        self.CanAttack = canAttack
end
 
function _MyHero:HeroCanMove()
        return not self.InStandZone and self.CanMove
end
 
function _MyHero:CheckStopMovement()
        if not MyHero:HeroCanMove() and not self.HasStopped then
                myHero:HoldPosition()
                self.HasStopped = true
        end
end
 
--[[
                _Crosshair Class
        ]]
 
class '_Crosshair' Crosshair = nil
 
--[[
                Initialise _Crosshair class
 
                damageType      DAMAGE_PHYSICAL or DAMAGE_MAGIC
                attackRange     Integer
                skillRange              Integer
                targetFocused   Boolean. Whether targets selected with left click should be focused.
                isCaster                Boolean. Whether spells should be prioritised over auto attacks.
]]
 
function _Crosshair:__init(damageType, attackRange, skillRange, targetFocused, isCaster)
        self.DamageType = damageType and damageType or DAMAGE_PHYSICAL
        self.AttackRange = attackRange
        self.SkillRange = skillRange
        self.TargetFocused = targetFocused
        self.IsCaster = isCaster
        self.Target = nil
        self.TargetMinion = nil
        self.Attack_Crosshair = TargetSelector(TARGET_LOW_HP_PRIORITY, attackRange, DAMAGE_PHYSICAL, self.TargetFocused)
        self.Skills_Crosshair = TargetSelector(TARGET_LOW_HP_PRIORITY, skillRange, self.DamageType, self.TargetFocused)
        self.Attack_Crosshair:SetBBoxMode(true)
        self.Attack_Crosshair:SetDamages(0, myHero.totalDamage, 0)
        self:ArrangePriorities()
        self.RangeScaling = true
        Crosshair = self
 
        self:UpdateCrosshairRange()
        self:LoadTargetSelector()
 
        AddTickCallback(function() self:_OnTick() end)
        AddUnloadCallback(function() self:_OnUnload() end)
end
 
function _Crosshair:_OnTick()
        self.Attack_Crosshair:update()
        if self.Attack_Crosshair.range ~= MyHero.TrueRange then
                self.Attack_Crosshair.range = MyHero.TrueRange
        end
        if self.Attack_Crosshair.target then
                self.Target = self.Attack_Crosshair.target
        else
                self.Skills_Crosshair:update()
                self.Target = self.Skills_Crosshair.target
        end
        if ConfigMenu.Focused ~= self.TargetFocused then
                self.TargetFocused = ConfigMenu.Focused
                self.Attack_Crosshair.targetSelected = self.TargetFocused
                self.Skills_Crosshair.targetSelected = self.targetFocused
        end
        self.TargetMinion = Minions.Target
end
 
function _Crosshair:_OnUnload()
        self:SaveTargetSelector()
end
 
function _Crosshair:GetTarget()
        if ValidTarget(self.Attack_Crosshair.target) and not self.IsCaster then
                return self.Attack_Crosshair.target
        elseif ValidTarget(self.Skills_Crosshair.target) then
                return self.Skills_Crosshair.target
        end
end
 
function _Crosshair:HasOrbwalkTarget()
        return self and self.Target and self.Attack_Crosshair.Target and self.Target == self.Attack_Crosshair.target
end
 
function _Crosshair:ArrangePriorities()
        if #GetEnemyHeroes() < 5 then return end
        for _, Champion in pairs(Data.ChampionData) do
                TS_SetHeroPriority(Champion.Priority, Champion.Name)
        end
end
 
function _Crosshair:SetSkillCrosshairRange(Range)
        self.RangeScaling = false
        self.Skills_Crosshair.range = Range
end
 
function _Crosshair:UpdateCrosshairRange()
        for _, Skill in pairs(Skills.SkillsList) do
                if Skill:GetRange() > self.Skills_Crosshair.range then
                        self.Skills_Crosshair.range = Skill:GetRange()
                end
        end
end
 
function _Crosshair:SaveTargetSelector()
        local save = GetSave("SidasAutoCarry")
        save.TargetSelectorMode = Crosshair.Attack_Crosshair.mode
        save:Save()
end
 
function _Crosshair:LoadTargetSelector()
        local save = GetSave("SidasAutoCarry")
        if save.TargetSelectorMode then
                Crosshair.Attack_Crosshair.mode = save.TargetSelectorMode
                Crosshair.Skills_Crosshair.mode = save.TargetSelectorMode
        end
end
 
--[[
                _Minions Class
]]
 
class '_Minions' Minions = nil
 
function _Minions:__init()
        self.ScanRange = 3000
        self.MinionHealthBuffer = 1.5
        self.EnemyMinions = minionManager(MINION_ENEMY, MyHero.TrueRange + 65, myHero, MINION_SORT_HEALTH_ASC)
    self.AllyMinions = minionManager(MINION_ALLY, self.ScanRange, myHero, MINION_SORT_HEALTH_ASC)
    self.IncomingDamage = {}
    Minions = self
 
    AddTickCallback(function() self:_OnTick() end)
    AddProcessSpellCallback(function(Unit, Spell)self:_OnProcessSpell(Unit, Spell) end)
end
 
function _Minions:_OnTick()
        self.EnemyMinions:update()
        self.AllyMinions:update()
 
        if not Orbwalker:CanOrbwalkTarget(self.KillableMinion) then
                local _Menu = MenuManager:GetActiveMenu()
                if _Menu and ConfigMenu.Freeze and (_Menu.name ~= "sidasaclaneclear") then
                        Killable, AlmostKillable = self:GetKillableMinionFreeze()
                else
                        Killable, AlmostKillable = self:GetKillableMinion()
                end
               
                if Orbwalker:CanOrbwalkTarget(Killable) then
                        self.KillableMinion = Killable
                elseif Orbwalker:CanOrbwalkTarget(AlmostKillable) then
                        self.AlmostKillable = AlmostKillable
                else
                        self.KillableMinion = nil
                        self.AlmostKillable = nil
                end
        end
end
 
function _Minions:_OnProcessSpell(Unit, Spell)
        if Unit and Spell.target and Unit.team == myHero.team and (Data.MinionData[Unit.charName] or Data.MinionData[Unit.type]) and GetDistance(Unit) <= self.ScanRange then
                self.IncomingDamage[Unit.networkID] = _Attack(Unit, Spell)
        end
end
 
function _Minions:LaneClear()
        if Orbwalker:CanOrbwalkTarget(self.KillableMinion) then
                Orbwalker:Orbwalk(self.KillableMinion)
        elseif self.AlmostKillable then
                MyHero:Move()
        elseif Structures:CanOrbwalkStructure() then
                Orbwalker:OrbwalkIgnoreChecks(Structures:GetTargetStructure())
        else
                Orbwalker:Orbwalk(self:GetSecondLowestHealthMinion())
        end
end
 
function _Minions:LastHit()
        Orbwalker:Orbwalk(self.KillableMinion)
end
 
function _Minions:GetPredictedDamage(Attack)
        if not ValidTarget(Attack.Source, self.ScanRange, false) or not ValidTarget(Attack.Target) or Helper.Tick > Attack.LandsAt then
                self.IncomingDamage[Attack.Source.networkID] = nil
                return 0, 0
        elseif GetDistance(Attack.Source, Attack.Origin) > 3 then
                return 0, 0
        else
                local TimeToHit = 0
                if MyHero.IsMelee then
                        TimeToHit = Orbwalker.LastWindUp
                else
                        TimeToHit = MyHero:GetTimeToHitTarget(Attack.Target)
                end
                if TimeToHit > Attack.LandsAt then
                        return Attack.Damage, Attack.Damage
                else
                        return 0, Attack.Damage
                end
        end
end
 
function _Minions:GetKillableMinion(Freeze)
        for _, EnemyMinion in ipairs(self.EnemyMinions.objects) do
                local Damage, TotalDamage, MyDamage = 0, 0, 0
 
                if Freeze then
                        MyDamage = MyHero:GetTotalAttackDamageAgainstTarget(EnemyMinion, true)
                        MyDamage = MyDamage < 50 and MyDamage or 50
                else
                        MyDamage = MyHero:GetTotalAttackDamageAgainstTarget(EnemyMinion, true)
                end
 
                if Orbwalker:CanOrbwalkTarget(EnemyMinion) or (MyHero.IsMelee and GetDistance(EnemyMinion) < MyHero.TrueRange + 75) then
                        if EnemyMinion.health < MyDamage then
                                return EnemyMinion, nil
                        else
                                for _, Attack in pairs(self.IncomingDamage) do
                                        if Attack.Target.networkID == EnemyMinion.networkID then
                                                local _Damage, _TotalDamage = self:GetPredictedDamage(Attack)
                                                Damage = Damage + _Damage
                                                TotalDamage = TotalDamage + _TotalDamage
                                        end
                                end
 
                                if EnemyMinion.health - Damage + self.MinionHealthBuffer < MyDamage then
                                        return EnemyMinion, nil
                                end
                                if not Freeze then
                                        for _, func in pairs(Plugins.RegisteredBonusLastHitDamage) do
                                                if EnemyMinion.health - Damage + self.MinionHealthBuffer < MyDamage + func(EnemyMinion) then
                                                        return EnemyMinion, nil
                                                end
                                        end
                                end
                                if EnemyMinion.health - TotalDamage < MyDamage then
                                        return nil, EnemyMinion
                                end
                        end
                end
        end
end
 
function _Minions:GetKillableMinionFreeze()
        return self:GetKillableMinion(true)
end
 
function _Minions:GetLowestHealthMinion()
        for i =1, #self.EnemyMinions.objects, 1 do
                local Minion = self.EnemyMinions.objects[i]
                if Orbwalker:CanOrbwalkTarget(Minion) then
                        return Minion
                end
        end
end
 
function _Minions:GetSecondLowestHealthMinion()
        local found = nil
        for i =1, #self.EnemyMinions.objects, 1 do
                local Minion = self.EnemyMinions.objects[i]
                if Orbwalker:CanOrbwalkTarget(Minion) and found then
                        return Minion
                elseif Orbwalker:CanOrbwalkTarget(Minion) then
                        found = Minion
                end
        end
        return found
end
 
--[[
                _Attack Class
]]
 
class '_Attack'
 
function _Attack:__init(Source, Spell)
        self.Source = Source
        self.Target = Spell.target
        self.Damage = Source:CalcDamage(self.Target)
        self.Started = Helper.Tick
        self.Delay = Spell.windUpTime * 1000
        self.Speed = Data.MinionData[Source.charName] and Data.MinionData[Source.charName].ProjectileSpeed or Data.MinionData[Source.type].ProjectileSpeed
        self.LandsAt = ((self.Speed == 0 and self.Delay or self.Delay + GetDistance(Source, self.Target) / self.Speed) + Helper.Tick)
        self.FiresAt = Helper.Tick + self.Delay
        self.Origin = {x = Source.x, z = Source.z}
        self.IsTowerShot = false
end
 
--[[
                _Jungle Class
]]
 
class '_Jungle' Jungle = nil
 
function _Jungle:__init()
        self.JungleMonsters = {}
        Jungle = self
        for i = 0, objManager.maxObjects do
        local object = objManager:getObject(i)
                if Data:IsJungleMinion(object) then
                        table.insert(self.JungleMonsters, object)
                end
        end
 
        AddCreateObjCallback(function(Object) self:_OnCreateObj(Object) end)
        AddDeleteObjCallback(function(Object) self:_OnDeleteObj(Object) end)
end
 
function _Jungle:_OnCreateObj(Object)
        if Data:IsJungleMinion(Object) then
                table.insert(self.JungleMonsters, Object)
        end
end
 
function _Jungle:_OnDeleteObj(Object)
        if Data:IsJungleMinion(Object) then
                for i, Obj in pairs(self.JungleMonsters) do
                        if obj == Object then
                                table.remove(self.JungleMonsters, i)
                        end
                end
        end
end
 
function _Jungle:GetJungleMonsters()
        return self.JungleMonsters
end
 
function _Jungle:GetAttackableMonster()
        local HighestPriorityMonster =  nil
        local Priority = 0
        for _, Monster in pairs(self.JungleMonsters) do
                if Orbwalker:CanOrbwalkTarget(Monster) then
                        local CurrentPriority = Data:GetJunglePriority(Monster.name)
                        if Monster.health < MyHero:GetTotalAttackDamageAgainstTarget(Monster) then
                                return Monster
                        elseif not HighestPriorityMonster then
                                HighestPriorityMonster = Monster
                                Priority = CurrentPriority
                        else
                                if CurrentPriority < Priority then
                                        HighestPriorityMonster = Monster
                                        Priority = CurrentPriority
                                end
                        end
                end
        end
        return HighestPriorityMonster
end
 
class '_Structures' Structures = nil
 
function _Structures:__init()
        Structures = self
        self.TowerCollisionRange = 88.4
        self.InhibCollisionRange = 205
        self.NexusCollisionRange = 300
end
 
function _Structures:TowerTargetted()
        return GetTarget() and GetTarget().type == "obj_AI_Turret" and GetTarget().team ~= myHero.team
end
 
function _Structures:InhibTargetted()
        return GetTarget() and GetTarget().type == "obj_BarracksDampener" and GetTarget().team ~= myHero.team
end
 
function _Structures:NexusTargetted()
        return GetTarget() and GetTarget().type == "obj_HQ" and GetTarget().team ~= myHero.team
end
 
function _Structures:CanOrbwalkStructure()
        return self:CanOrbwalkTower() or self:CanOrbwalkInhib() or self:CanOrbwalkNexus()
end
 
function _Structures:GetTargetStructure()
        return GetTarget()
end
 
function _Structures:CanOrbwalkTower()
        return self:TowerTargetted() and GetDistance(GetTarget()) - self.TowerCollisionRange < MyHero.TrueRange
end
 
function _Structures:CanOrbwalkInhib()
        return self:InhibTargetted() and GetDistance(GetTarget()) - self.InhibCollisionRange < MyHero.TrueRange
end
 
function _Structures:CanOrbwalkNexus()
        return self:NexusTargetted() and GetDistance(GetTarget()) - self.NexusCollisionRange < MyHero.TrueRange
end
 
--[[
                _Skills Class
]]
 
class '_Skills' Skills = nil
 
function _Skills:__init()
        self.SkillsList = {}
        Skills = self
        if VIP_USER then require "Collision" end
        AddTickCallback(function() self:_OnTick() end)
end
 
function _Skills:_OnTick()
        for _, Skill in pairs(self.SkillsList) do
                if Keys.AutoCarry and AutoCarryMenu[Skill.RawName.."AutoCarry"] or
                        Keys.MixedMode and MixedModeMenu[Skill.RawName.."MixedMode"] or
                        Keys.LaneClear and LaneClearMenu[Skill.RawName.."LaneClear"] then
                        Skill.Active = true
                else
                        Skill.Active = false
                end
        end
        self:SortSkillOrder()
end
 
function _Skills:CastAll(Target)
        for _, Skill in ipairs(self.SkillsList) do
                if Skill.Enabled then
                        Skill:Cast(Target)
                end
        end
end
 
function _Skills:GetSkill(Key)
        for _, Skill in pairs(self.SkillsList) do
                if Skill.Key == Key then
                        return Skill
                end
        end
end
 
function _Skills:GetSkillMenu(Key)
        if MenuManager.SkillMenu[Key] then
                return MenuManager.SkillMenu[Key]
        else
                return {CastPrio = 0}
        end
end
 
function _Skills:SortSkillOrder()
        table.sort(self.SkillsList, function(a, b) return self:GetSkillMenu(a.Key).CastPrio < self:GetSkillMenu(b.Key).CastPrio end)
end
 
function _Skills:HasSkillReady()
        for _, Skill in pairs(self.SkillsList) do
                if Skill.Ready then
                        return true
                end
        end
end
 
function _Skills:NewSkill(enabled, key, range, displayName, type, minMana, afterAttack, reqAttackTarget, speed, delay, width, collision)
        return _Skill(enabled, key, range, displayName, type, minMana, afterAttack, reqAttackTarget, speed, delay, width, collision, true)
end
 
function _Skills:DisableAll()
        for _, Skill in pairs(self.SkillsList) do
                Skill.Enabled = false
        end
end
 
--[[
                _Skill Class
]]
 
class '_Skill'
 
SPELL_TARGETED = 1
SPELL_LINEAR = 2
SPELL_CIRCLE = 3
SPELL_CONE = 4
SPELL_LINEAR_COL = 5
SPELL_SELF = 6
SPELL_SELF_AT_MOUSE = 7
AutoCarry.SPELL_TARGETED = 1
AutoCarry.SPELL_LINEAR = 2
AutoCarry.SPELL_CIRCLE = 3
AutoCarry.SPELL_CONE = 4
AutoCarry.SPELL_LINEAR_COL = 5
AutoCarry.SPELL_SELF = 6
AutoCarry.SPELL_SELF_AT_MOUSE = 7
 
-- --[[
--              Initialise _Skill class
 
--              enabled                         Boolean - set true for auto carry to automatically cast it, false for manual control in plugin
--              key                             Spell key, e.g _Q
--              range                           Spell range
--              displayName             The name to display in menus
--              type                            SPELL_TARGETED, SPELL_LINEAR, SPELL_CIRCLE, SPELL_CONE, SPELL_LINEAR_COL, SPELL_SELF, SPELL_SELF_AT_MOUSE
--              minMana                         Minimum percentage mana before cast is allowed
--              afterAttack             Boolean - set true to only cast right after an auto attack
--              reqAttackTarget         Boolean - set true to only cast if a target is in attack range
--              speed                           Speed of the projectile for skillshots
--              delay                           Delay of the spell for skillshots
--              width                           Width of the projectile for skillshots
--              collision                       Boolean - set true to check minion collision before casting
 
-- ]]
function _Skill:__init(enabled, key, range, displayName, type, minMana, afterAttack, reqAttackTarget, speed, delay, width, collision, custom)
        self.Key = key
        self.Range = range
        self.DisplayName = displayName
        self.RawName = self.DisplayName:gsub("[^A-Za-z0-9]", "")
        self.Type = type
        self.MinMana = minMana or 0
        self.AfterAttack = afterAttack or false
        self.ReqAttackTarget = reqAttackTarget or false
        self.Speed = speed or 0
        self.Delay = delay or 0
        self.Width = width or 0
        self.Collision = collision
        self.IsCustom = custom
        self.Active = true
        self.Enabled = enabled or false
        self.Ready = false
        if VIP_USER then
                self.KloPredCallback = function(unit, pos, castSpell)
                        if GetDistanceSqr(pos, castSpell.Source) < castSpell.RangeSqr and  myHero:CanUseSpell(castSpell.Name) == READY and myHero:GetSpellData(castSpell.Name).mana < myHero.mana then
                                if (self.Collision and not self:GetCollision(pos)) or not self.Collision then
                                CastSpell(castSpell.Name, pos.x, pos.z)
                        end
                    end
                end
               
                self.KloPred = ProdictManager.GetInstance():AddProdictionObject(self.Key, self.Range, self.Speed * 1000, self.Delay / 1000, self.Width, myHero, self.KloPredCallback)
        end
 
        AddTickCallback(function() self:_OnTick() end)
 
        table.insert(Skills.SkillsList, self)
end
 
function _Skill:_OnTick()
        if self.Enabled then
                if Skills:GetSkillMenu(self.Key).ManualCast and Crosshair:GetTarget() then
                        self:ForceCast(Crosshair:GetTarget())
                end
        end
        self.Ready = myHero:CanUseSpell(self.Key) == READY
        if self.Enabled and not self.IsCustom then
                self.reqAttackTarget = Skills:GetSkillMenu(self.Key).RequiresTarget
        end
end
 
function _Skill:Cast(Target, ForceCast)
        if not ForceCast then
                if (not self.Active and self.Enabled) or (not self.Enabled and not self.IsCustom) then
                        return
                elseif self.AfterAttack and not Orbwalker:IsAfterAttack() then
                        return
                elseif not self:ValidSkillTarget(Target) then
                        return
                elseif (self.ReqAttackTarget and not Orbwalker:CanOrbwalkTarget(Target)) then
                        return
                end
        end
        if not self:IsReady() then
                return
        end
 
 
        if self.Type == SPELL_SELF then
                CastSpell(self.Key)
        elseif self.Type == SPELL_SELF_AT_MOUSE then
                CastSpell(self.Key, mousePos.x, mousePos.z)
        elseif self.Type == SPELL_TARGETED then
                if ValidTarget(Target, self.Range) then
                        CastSpell(self.Key, Target)
                end
        elseif self.Type == SPELL_CONE then
                if ValidTarget(Target, self.Range) then
                        if not VIP_USER then
                                local predPos = self:GetPrediction(Target)
                                if predPos then
                                        CastSpell(self.Key, predPos.x, predPos.z)
                                end
                        else
                                self.KloPred:EnableTarget(Target, true)
                        end
                end
        elseif self.Type == SPELL_LINEAR or self.Type == SPELL_CIRCLE then
                if ValidTarget(Target) then
                        if not VIP_USER then
                                local predPos = self:GetPrediction(Target)
                                if predPos then
                                        CastSpell(self.Key, predPos.x, predPos.z)
                                end
                        else
                                self.KloPred:EnableTarget(Target, true)
                        end
                end
        elseif self.Type == SPELL_LINEAR_COL then
                if ValidTarget(Target) then
                       
                        if self.Collision then
                                if not VIP_USER then
                                        local predPos = self:GetPrediction(Target)
                                        if predPos then
                                                local collision = self:GetCollision(predPos)
                                                if not collision or ForceCast then
                                                        CastSpell(self.Key, predPos.x, predPos.z)
                                                end
                                        end
                                else
                                        self.KloPred:EnableTarget(Target, true)
                                end
                        end
                end
        end
end
 
function _Skill:ForceCast(Target)
        self:Cast(Target, true)
end
 
function _Skill:GetPrediction(Target)
        if VIP_USER then
                pred = TargetPredictionVIP(self.Range, self.Speed*1000, self.Delay/1000, self.Width)
                if pred and pred:GetHitChance(Target) > ConfigMenu.HitChance/100 then
                        return pred:GetPrediction(Target)
                end
        elseif not VIP_USER then
                pred = TargetPrediction(self.Range, self.Speed, self.Delay, self.Width)
                return pred:GetPrediction(Target)
        end
end
 
function _Skill:GetCollision(pos)
        if VIP_USER and self.Collision then
                local col = Collision(self.Range, self.Speed*1000, self.Delay/1000, self.Width)
                return col:GetMinionCollision(myHero, pos)
        elseif self.Collision then
                for _, Minion in pairs(Minions.EnemyMinions.objects) do
                        if ValidTarget(Minion) and myHero.x ~= Minion.x then
                                local myX = myHero.x
                                local myZ = myHero.z
                                local tarX = pos.x
                                local tarZ = pos.z
                                local deltaX = myX - tarX
                                local deltaZ = myZ - tarZ
                                local m = deltaZ/deltaX
                                local c = myX - m*myX
                                local minionX = Minion.x
                                local minionZ = Minion.z
                                local distanc = (math.abs(minionZ - m*minionX - c))/(math.sqrt(m*m+1))
                                if distanc < self.Width and ((tarX - myX)*(tarX - myX) + (tarZ - myZ)*(tarZ - myZ)) > ((tarX - minionX)*(tarX - minionX) + (tarZ - minionZ)*(tarZ - minionZ)) then
                                        return true
                                end
                        end
           end
           return false
        end
end
 
function _Skill:GetHitChance(pred)
        if VIP_USER then
                return pred:GetHitChance(target) > ConfigMenu.HitChance/100
        end
end
 
function _Skill:ValidSkillTarget(Target)
        if not self.IsCustom then
                local _Menu = Skills:GetSkillMenu(self.Key)
                if not Target or myHero.mana / myHero.maxMana * 100 < _Menu.MinMana or Target.health / Target.maxHealth * 100 < _Menu.MinHealth or Target.health / Target.maxHealth * 100 > _Menu.MaxHealth then
                        return false
                end
                return true
        end
        return true
end
 
function _Skill:GetRange()
        return self.reqAttackTarget and MyHero.TrueRange or self.Range
end
 
function _Skill:IsReady()
        return myHero:CanUseSpell(self.Key) == READY
end
 
--[[
                _Items Class
]]
 
class '_Items' Items = nil
 
function _Items:__init()
        self.ItemList = {}
        Items = self
 
        AddTickCallback(function() self:_OnTick() end)
end
 
function _Items:_OnTick()
        for _, Item in pairs(self.ItemList) do
                if Keys.AutoCarry and AutoCarryMenu[Item.RawName.."AutoCarry"] or
                        Keys.MixedMode and MixedModeMenu[Item.RawName.."MixedMode"] or
                        Keys.LaneClear and LaneClearMenu[Item.RawName.."LaneClear"] then
                        Item.Active = true
                else
                        Item.Active = false
                end
        end
end
 
function _Items:UseAll(Target)
        if Target and Target.type == myHero.type then
                for _, Item in pairs(self.ItemList) do
                        Item:Use(Target)
                end
        end
end
 
function _Items:UseItem(ID, Target)
        for _, Item in pairs(self.ItemList) do
                if Item.ID == ID then
                        Item:Use(Target)
                end
        end
end
 
function _Items:GetItem(ID)
        for _, Item in pairs(self.ItemList) do
                if Item.ID == ID then
                        return Item
                end
        end
end
 
function _Items:GetBotrkBonusLastHitDamage(StartingDamage, Target)
        local _BonusDamage = 0
        if GetInventoryHaveItem(3153) then
                if ValidTarget(Target) then
                        _BonusDamage = Target.health / 20
                        if _BonusDamage >= 60 then
                                _BonusDamage = 60
                        end
                end
        end
        return _BonusDamage
end
 
--[[
                _Item Class
]]
 
class '_Item'
 
--TODO: Add Muramana
function _Item:__init(_Name, _ID, _RequiresTarget, _Range, _Override)
        self.Name = _Name
        self.RawName = self.Name:gsub("[^A-Za-z0-9]", "")
        self.ID = _ID
        self.RequiresTarget = _RequiresTarget
        self.Range = _Range
        self.Slot = nil
        self.Override = _Override
        self.Active = true
        self.Enabled = true
 
        table.insert(Items.ItemList, self)
end
 
function _Item:Use(Target)
        if self.Override then
                return self.Override()
        end
        if self.RequiresTarget and not Target then
                return
        end
        if not self.Active or not self.Enabled then
                return
        end
 
        self.Slot = GetInventorySlotItem(self.ID)
 
        if self.Slot then      
                if self.ID == 3153 then -- BRK
                        local _Menu = MenuManager:GetActiveMenu()
                        if _Menu and _Menu.botrkSave then
                                if  myHero.health <= myHero.maxHealth * 0.65 then
                                        CastSpell(self.Slot, Target)
                                end
                        else
                                CastSpell(self.Slot, Target)
                        end
                elseif self.ID == 3042 then -- Muramana
                        if not MuramanaIsActive() then
                                MuramanaOn()
                        end
                elseif not self.RequiresTarget and Orbwalker:CanOrbwalkTarget(Target) then
                        CastSpell(self.Slot)
                elseif self.RequiresTarget and ValidTarget(Target) and GetDistance(Target) <= self.Range then
                        CastSpell(self.Slot, Target)
                end
        end
end
 
--[[
                _Helper Class
]]
 
class '_Helper' Helper = nil
 
function _Helper:__init()
        self.Tick = 0
        self.Latency = 0
        self.Colour = {Green = 0x00FF00}
        self.EnemyTable = {}
        Helper = self
        self.EnemyTable = GetEnemyHeroes()
        AddTickCallback(function() self:_OnTick() end)
end
 
function _Helper:_OnTick()
        self.Tick = GetTickCount()
        self.Latency = GetLatency()
end
 
function _Helper:StringContains(string, contains)
        return string:lower():find(contains)
end
 
function _Helper:DrawCircleObject(Object, Range, Colour, Thickness)
        Thickness = Thickness and Thickness or 0
        for i = 0, Thickness do
                if DrawingMenu.LowFPS then
                        self:DrawCircle2(Object.x, Object.y, Object.z, Range + i, Colour)
                else
                        DrawCircle(Object.x, Object.y, Object.z, Range + i, Colour)
                end
        end
end
 
-- Low fps circles by barasia, vadash and viseversa
function _Helper:DrawCircleNextLvl(x, y, z, radius, width, color, chordlength)
    radius = radius or 300
                quality = math.max(8,self:round(180/math.deg((math.asin((chordlength/(2*radius)))))))
                quality = 2 * math.pi / quality
                radius = radius*.92
    local points = {}
    for theta = 0, 2 * math.pi + quality, quality do
        local c = WorldToScreen(D3DXVECTOR3(x + radius * math.cos(theta), y, z - radius * math.sin(theta)))
        points[#points + 1] = D3DXVECTOR2(c.x, c.y)
    end
    DrawLines2(points, width or 1, color or 4294967295)
end
 
function _Helper:round(num)
        if num >= 0 then return math.floor(num+.5) else return math.ceil(num-.5) end
end
 
function _Helper:DrawCircle2(x, y, z, radius, color)
    local vPos1 = Vector(x, y, z)
    local vPos2 = Vector(cameraPos.x, cameraPos.y, cameraPos.z)
    local tPos = vPos1 - (vPos1 - vPos2):normalized() * radius
    local sPos = WorldToScreen(D3DXVECTOR3(tPos.x, tPos.y, tPos.z))
    if OnScreen({ x = sPos.x, y = sPos.y }, { x = sPos.x, y = sPos.y }) then
        self:DrawCircleNextLvl(x, y, z, radius, 1, color, 75)  
    end
end
 
function _Helper:GetHitBoxDistance(Target)
        return GetDistance(Target) - GetDistance(Target, Target.minBBox)
end
 
function _Helper:TrimString(s)
        return s:find'^%s*$' and '' or s:match'^%s*(.*%S)'
end
 
function _Helper:GetClasses()
        return AutoCarry.Skills, AutoCarry.Keys, AutoCarry.Items, AutoCarry.Data, AutoCarry.Jungle, AutoCarry.Helper, AutoCarry.MyHero, AutoCarry.Minions, AutoCarry.Crosshair, AutoCarry.Orbwalker
end
 
function _Helper:ArgbFromMenu(menu)
        return ARGB(menu[1], menu[2], menu[3], menu[4])
end
 
function _Helper:DecToHex(Dec)
        local B, K, Hex, I, D = 16, "0123456789ABCDEF", "", 0
        while Dec > 0 do
                I = I + 1
                Dec, D = math.floor(Dec / B), math.fmod(Dec, B) + 1
                Hex = string.sub(K, D, D)..Hex
        end
        return Hex
end
 
function _Helper:HexFromMenu(menu)
        local argb = {}
        argb["a"] = menu[1]
        argb["r"] = menu[2]
        argb["g"] = menu[3]
        argb["b"] = menu[4]
        return tonumber(self:DecToHex(argb["a"]) .. self:DecToHex(argb["r"]) .. self:DecToHex(argb["g"]) .. self:DecToHex(argb["b"]), 16);
end
 
function _Helper:IsEvading()
        return _G.evade
end
 
--[[
                Keys Class
]]
 
class '_Keys' Keys = nil
 
function _Keys:__init()
        self.KEYS_KEY = 0
        self.KEYS_MENUKEY = 1
        self.AutoCarry = false
        self.MixedMode = false
        self.LastHit = false
        self.LaneClear = false
        self.AutoCarryKeys = {}
        self.MixedModeKeys = {}
        self.LastHitKeys = {}
        self.LaneClearKeys = {}
        Keys = self
 
        AddTickCallback(function() self:_OnTick() end)
end
 
function _Keys:_OnTick()
        self.AutoCarry = self:IsKeyEnabled(self.AutoCarryKeys)
        self.MixedMode = self:IsKeyEnabled(self.MixedModeKeys)
        self.LastHit = self:IsKeyEnabled(self.LastHitKeys)
        self.LaneClear = self:IsKeyEnabled(self.LaneClearKeys)
end
 
function _Keys:IsKeyEnabled(List)
        for _, Key in pairs(List) do
                if Key.Type == self.KEYS_KEY then
                        if IsKeyDown(Key.Key) then
                                return true
                        end
                elseif Key.Type == self.KEYS_MENUKEY then
                        if Key.Menu[Key.Param] then
                                return true
                        end
                end
        end
        return false
end
 
function _Keys:RegisterMenuKey(Menu, Param, Mode)
        local MenuKey = _MenuKey(Menu, Param)
        self:Insert(MenuKey, Mode)
end
 
function _Keys:RegisterKey(key, Mode)
        local Key = _Key(key)
        self:Insert(Key, Mode)
end
 
function _Keys:Insert(Key, Mode)
        if Mode == MODE_AUTOCARRY then
                table.insert(self.AutoCarryKeys, Key)
        elseif Mode == MODE_MIXEDMODE then
                table.insert(self.MixedModeKeys, Key)
        elseif Mode == MODE_LASTHIT then
                table.insert(self.LastHitKeys, Key)
        elseif Mode == MODE_LANECLEAR then
                table.insert(self.LaneClearKeys, Key)
        end
end
 
--[[
                Key Class
]]
 
class '_Key'
 
function _Key:__init(key)
        self.Key = key
        self.Type = Keys.KEYS_KEY
end
 
--[[
                MenuKey Class
]]
 
class '_MenuKey'
 
function _MenuKey:__init(menu, param)
        self.Menu = menu
        self.Param = param
        self.Type = Keys.KEYS_MENUKEY
end
 
--[[
                _SwingTimer Class
]]
 
class '_SwingTimer' SwingTimer = nil
 
function _SwingTimer:__init()
        self.bar_green = GetSprite("SidasAutoCarry\\bar_green.dds")
        self.bar_red = GetSprite("SidasAutoCarry\\bar_red.dds")
        self.Width = 300
        self.X = WINDOW_W/2 - (self.Width/2)
        self.Y = WINDOW_H/2
        self.Height = self.bar_green.height    
        self.ResizeButtonWidth = 30
        self.Moving = false
        self.Resizing = false
        self.Save = GetSave("SidasAutoCarry").SwingTimer
        SwingTimer = self
 
        if self.Save then
                self.X = self.Save.X
                self.Y = self.Save.Y
                self.Width = self.Save.Width
        end
        AddMsgCallback(function(Msg, Key) self:_OnWndMsg(Msg, Key) end)
        AddDrawCallback(function() self:_OnDraw() end)
        AddTickCallback(function() self:_OnTick() end)
end
 
function _SwingTimer:_OnDraw()
        if not ExtrasMenu.ShowSwingTimer then return end
        local Difference = (Helper.Tick - Orbwalker.LastAttack) / (Orbwalker:GetNextAttackTime() - Orbwalker.LastAttack)
        if Difference > 1 or Difference < 0 then
                self:_DrawBar(self.X, self.Y, self.Width, 1)
        else
                self:_DrawBar(self.X, self.Y, self.Width, Difference)
        end
        if IsKeyDown(16) then
                DrawRectangleOutline(self.X + self.Width + 5, self.Y, self.ResizeButtonWidth, self.Height, ARGB(0xFF,0xFF,0xFF,0xFF), 3)
        end
end
 
function _SwingTimer:_OnWndMsg(Msg, Key)
        if Msg == WM_LBUTTONDOWN and IsKeyDown(16) then
                if CursorIsUnder(self.X, self.Y, self.Width, self.Height) then
                        self.Moving = true
                elseif CursorIsUnder(self.X + self.Width + 5, self.Y, self.ResizeButtonWidth, self.Height) then
                        self.Resizing = true
                end
        elseif Msg == WM_LBUTTONUP and (self.Moving or self.Resizing) then
                self.Moving = false
                self.Resizing = false
                GetSave("SidasAutoCarry").SwingTimer = {X = self.X, Y = self.Y, Height = self.Height, Width = self.Width}
        end
end
 
function _SwingTimer:_OnTick()
        if self.Moving then
                self.X = GetCursorPos().x - self.Width / 2
                self.Y = GetCursorPos().y
        elseif self.Resizing then
                self.Width = GetCursorPos().x - self.X - self.ResizeButtonWidth / 2
        end
end
 
function _SwingTimer:_DrawBar(x, y, barLen, percentage)
        DrawRectangleOutline(x-1, y-1, barLen+3, self.Height+2, ARGB(0x00,0xFF,0xFF,0xFF), 1)
    self.bar_green:DrawEx(Rect(0, 0, barLen*percentage, self.Height), D3DXVECTOR3(0,0,0), D3DXVECTOR3(x,y,0), 0xFF)
    self.bar_red:DrawEx(Rect(barLen*percentage, 0, barLen, self.Height), D3DXVECTOR3(0,0,0), D3DXVECTOR3(x+barLen*percentage,y,0), 0xFF)
    DrawTextA(math.floor(percentage*100).."%", 12, x+(barLen/2), y-1, ARGB(255,255,255,255), "center")
end
 
--[[
                _Streaming Class
]]
 
class '_Streaming'
 
function _Streaming:__init()
        self.Save = GetSave("SidasAutoCarry")
 
        AddTickCallback(function()self:_OnTick() end)
        AddMsgCallback(function(msg, key)self:_OnWndMsg(msg, key) end)
 
        if self.Save.StreamingMode then
                self:EnableStreaming()
        else
                self:DisableStreaming()
        end
end
 
function _Streaming:_OnTick()
        if self.StreamingMenu then
                self.StreamingMenu._param[4].text = self.StreamingMenu.Colour == 0 and "Green" or "Red"
        end
        if self.StreamEnabled then
                self:EnableStreaming()
        end
end
 
function _Streaming:_OnWndMsg(msg, key)
        if msg == KEY_DOWN and key == 118 then
                if not self.StreamEnabled then
                        self:EnableStreaming()
                else
                        self:DisableStreaming()
                end
        end
end
 
function _Streaming:EnableStreaming()
        self.Save.StreamingMode = true
        self.StreamEnabled = true
        if not self.ChatTimeout then
                self.ChatTimeout = GetTickCount() + 3000
        elseif GetTickCount() < self.ChatTimeout then
                self:DisableOverlay()
                for i = 0, 15 do
                        PrintChat("")
                end
        else
                _G.PrintChat = function() end
        end
end
 
function _Streaming:DisableStreaming()
        self.Save.StreamingMode = false
        self.StreamEnabled = false
        if self.ChatTimeout then
                EnableOverlay()
                self.ChatTimeout = nil
        end
end
 
function _Streaming:OnMove()
        if not self.MenuCreated then return end
        if self.StreamingMenu.MinRand > self.StreamingMenu.MaxRand then
                print("Reborn: Min cannot be higher than Max in Streaming Menu")
        elseif self.StreamingMenu.ShowClick and (not self.NextClick or Helper.Tick > self.NextClick) then
                if self.StreamingMenu.Colour == 0 then
                        ShowGreenClick(mousePos)
                else
                        ShowRedClick(mousePos)
                end
                self.nextClick = Helper.Tick + math.random(self.StreamingMenu.MinRand, self.StreamingMenu.MaxRand)
        end
end
 
function _Streaming:CreateMenu()
        self.StreamingMenu = scriptConfig("Sida's Auto Carry: Streaming", "sidasacstreaming")
        self.StreamingMenu:addParam("ShowClick", "Show Fake Click Marker", SCRIPT_PARAM_ONOFF, false)
        self.StreamingMenu:addParam("MinRand", "Minimum Time Between Clicks", SCRIPT_PARAM_SLICE, 150, 0, 1000, 0)
        self.StreamingMenu:addParam("MaxRand", "Maximum Time Between Clicks", SCRIPT_PARAM_SLICE, 650, 0, 1000, 0)
        self.StreamingMenu:addParam("Colour", "Green", SCRIPT_PARAM_SLICE, 0, 0, 1, 0)
        self.StreamingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        self.StreamingMenu:addParam("sep", "Toggle Streaming Mode with F7", SCRIPT_PARAM_INFO, "")
        self.MenuCreated = true
end
 
function _Streaming:DisableOverlay()
    _G.DrawText,
    _G.PrintFloatText,
    _G.DrawLine,
    _G.DrawArrow,
    _G.DrawCircle,
    _G.DrawRectangle,
    _G.DrawLines,
    _G.DrawLines2 = function() end,
    function() end,
    function() end,
    function() end,
    function() end,
    function() end,
    function() end
end
 
Streaming = _Streaming()
 
--[[
                _MenuManager Class
]]
 
--[[
                _Summoner Class
]]
 
class '_Summoner'
 
function _Summoner:__init()
 
end
 
class '_MenuManager' MenuManager = nil
 
function _MenuManager:__init()
        self.AutoCarry = false
        self.MixedMode = false
        self.LastHit = false
        self.LaneClear = false
        self.SkillMenu = {}
 
        AddTickCallback(function() self:OnTick() end)
        AddMsgCallback(function(msg, key) self:OnWndMsg(msg, key) end)
        AddUnloadCallback(function() self:_OnUnload() end)
        AddBugsplatCallback(function() self:_OnBugsplat() end)
        AddExitCallback(function() self:_OnExit() end)
        MenuManager = self
 
        --[[ Auto Carry Menu ]]
 
        AutoCarryMenu = scriptConfig("Sida's Auto Carry: Auto Carry", "sidasacautocarry")
        AutoCarryMenu:addParam("title", "              Sida's Auto Carry: Reborn", SCRIPT_PARAM_INFO, "")
        AutoCarryMenu:addParam("sep", "-- Settings--", SCRIPT_PARAM_INFO, "")
        AutoCarryMenu:addParam("Toggle", "Toggle mode (requires reload)", SCRIPT_PARAM_ONOFF, false)
        AutoCarryMenu:addParam("Active", "Auto Carry", AutoCarryMenu.Toggle and SCRIPT_PARAM_ONKEYTOGGLE or SCRIPT_PARAM_ONKEYDOWN, false, 219)
        AutoCarryMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        AutoCarryMenu:addParam("sep", "-- Skills --", SCRIPT_PARAM_INFO, "")
        AutoCarryMenu:permaShow("title")
        AutoCarryMenu:permaShow("Active")
        AutoCarryMenu:addTS(Crosshair.Attack_Crosshair)
        Keys:RegisterMenuKey(AutoCarryMenu, "Active", MODE_AUTOCARRY)
 
        if #Skills.SkillsList > 0 then
                for _, Skill in pairs(Skills.SkillsList) do
                        AutoCarryMenu:addParam(Skill.RawName.."AutoCarry", "Use "..Skill.DisplayName, SCRIPT_PARAM_ONOFF, self:LoadSkill(AutoCarryMenu, Skill.RawName, "AutoCarry"))
                end
        else
                AutoCarryMenu:addParam("sep", "No supported skills for "..myHero.charName, SCRIPT_PARAM_INFO, "")
        end
 
        AutoCarryMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        AutoCarryMenu:addParam("sep", "-- Items --", SCRIPT_PARAM_INFO, "")
        for _, Item in pairs(Items.ItemList) do
                AutoCarryMenu:addParam(Item.RawName.."AutoCarry", "Use "..Item.Name, SCRIPT_PARAM_ONOFF, true)
        end
        AutoCarryMenu:addParam("botrkSave", "Save BotRK for max heal", SCRIPT_PARAM_ONOFF, true)
 
 
        --[[ Last Hit Menu ]]
 
        LastHitMenu = scriptConfig("Sida's Auto Carry: Last Hit", "sidasaclasthit")
        LastHitMenu:addParam("sep", "-- Settings--", SCRIPT_PARAM_INFO, "")
        LastHitMenu:addParam("Toggle", "Toggle mode (requires reload)", SCRIPT_PARAM_ONOFF, false)
        LastHitMenu:addParam("Active", "Last Hit", LastHitMenu.Toggle and SCRIPT_PARAM_ONKEYTOGGLE or SCRIPT_PARAM_ONKEYDOWN, false, string.byte("A"))
        LastHitMenu:permaShow("Active")
        Keys:RegisterMenuKey(LastHitMenu, "Active", MODE_LASTHIT)
 
 
        --[[ Mixed Mode Menu ]]
 
        MixedModeMenu = scriptConfig("Sida's Auto Carry: Mixed Mode", "sidasacmixedmode")
        MixedModeMenu:addParam("sep", "-- Settings--", SCRIPT_PARAM_INFO, "")
        MixedModeMenu:addParam("Toggle", "Toggle mode (requires reload)", SCRIPT_PARAM_ONOFF, false)
        MixedModeMenu:addParam("Active", "Mixed Mode", MixedModeMenu.Toggle and SCRIPT_PARAM_ONKEYTOGGLE or SCRIPT_PARAM_ONKEYDOWN, false, string.byte("D"))
        MixedModeMenu:addParam("MinionPriority", "Prioritise Last Hit Over Harass", SCRIPT_PARAM_ONOFF, true)
        MixedModeMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        MixedModeMenu:addParam("sep", "-- Skills (Against Champions Only) --", SCRIPT_PARAM_INFO, "")
        MixedModeMenu:permaShow("Active")
        Keys:RegisterMenuKey(MixedModeMenu, "Active", MODE_MIXEDMODE)
 
        if #Skills.SkillsList > 0 then
                for _, Skill in pairs(Skills.SkillsList) do
                        MixedModeMenu:addParam(Skill.RawName.."MixedMode", "Use "..Skill.DisplayName, SCRIPT_PARAM_ONOFF, self:LoadSkill(MixedModeMenu, Skill.RawName, "MixedMode"))
                end
        else
                MixedModeMenu:addParam("sep", "No supported skills for "..myHero.charName, SCRIPT_PARAM_INFO, "")
        end
 
        MixedModeMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        MixedModeMenu:addParam("sep", "-- Items (Against Champions Only) --", SCRIPT_PARAM_INFO, "")
        for _, Item in pairs(Items.ItemList) do
                MixedModeMenu:addParam(Item.RawName.."MixedMode", "Use "..Item.Name, SCRIPT_PARAM_ONOFF, true)
        end
        MixedModeMenu:addParam("botrkSave", "Save BotRK for max heal", SCRIPT_PARAM_ONOFF, true)
       
 
        --[[ Lane Clear Menu ]]
 
        LaneClearMenu = scriptConfig("Sida's Auto Carry: Lane Clear", "sidasaclaneclear")
        LaneClearMenu:addParam("sep", "-- Settings--", SCRIPT_PARAM_INFO, "")
        LaneClearMenu:addParam("Toggle", "Toggle mode (requires reload)", SCRIPT_PARAM_ONOFF, false)
        LaneClearMenu:addParam("Active", "Lane Clear", LaneClearMenu.Toggle and SCRIPT_PARAM_ONKEYTOGGLE or SCRIPT_PARAM_ONKEYDOWN, false, string.byte("C"))
        LaneClearMenu:addParam("MinionPriority", "Prioritise Last Hit Over Harass", SCRIPT_PARAM_ONOFF, true)
        LaneClearMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        LaneClearMenu:addParam("sep", "-- Skills (Against Champions Only) --", SCRIPT_PARAM_INFO, "")
        LaneClearMenu:permaShow("Active")
        Keys:RegisterMenuKey(LaneClearMenu, "Active", MODE_LANECLEAR)
 
        if #Skills.SkillsList > 0 then
                for _, Skill in pairs(Skills.SkillsList) do
                        LaneClearMenu:addParam(Skill.RawName.."LaneClear", "Use "..Skill.DisplayName, SCRIPT_PARAM_ONOFF, self:LoadSkill(LaneClearMenu, Skill.RawName, "LaneClear"))   
                end
        else
                LaneClearMenu:addParam("sep", "No supported skills for "..myHero.charName, SCRIPT_PARAM_INFO, "")
        end
 
        LaneClearMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        LaneClearMenu:addParam("sep", "-- Items (Against Champions Only) --", SCRIPT_PARAM_INFO, "")
        for _, Item in pairs(Items.ItemList) do
                LaneClearMenu:addParam(Item.RawName.."LaneClear", "Use "..Item.Name, SCRIPT_PARAM_ONOFF, true)
        end
        LaneClearMenu:addParam("botrkSave", "Save BotRK for max heal", SCRIPT_PARAM_ONOFF, true)
 
        --[[ Skills Menus ]]
        for _, Skill in pairs(Skills.SkillsList) do
                self.SkillMenu[Skill.Key] = scriptConfig("Sida's Auto Carry: Skill "..Skill.DisplayName, Skill.RawName)
                self.SkillMenu[Skill.Key]:addParam("sep", "-- Against Champions --", SCRIPT_PARAM_INFO, "")
                self.SkillMenu[Skill.Key]:addParam("CastPrio", "Cast Priority (0 Is Cast First)", SCRIPT_PARAM_SLICE, 0, 0, 3, 0)
                self.SkillMenu[Skill.Key]:addParam("MinMana", "Mana Must Be Above %", SCRIPT_PARAM_SLICE, 0, 0, 100, 0)
                self.SkillMenu[Skill.Key]:addParam("MinHealth", "Enemy Health Must Be Above %", SCRIPT_PARAM_SLICE, 0, 0, 100, 0)
                self.SkillMenu[Skill.Key]:addParam("MaxHealth", "Enemy Health Must Be Below %", SCRIPT_PARAM_SLICE, 100, 0, 100, 0)
                self.SkillMenu[Skill.Key]:addParam("RequiresTarget", "Target Must Be In Attack Range", SCRIPT_PARAM_ONOFF, Skill.ReqAttackTarget)
                self.SkillMenu[Skill.Key]:addParam("ManualCast", "Manual Cast", SCRIPT_PARAM_ONKEYDOWN, false, 119)
                -- self.SkillMenu[Skill.Key]:addParam("sep", "", SCRIPT_PARAM_INFO, "")
                -- self.SkillMenu[Skill.Key]:addParam("sep", "-- Against Minions --", SCRIPT_PARAM_INFO, "")
                -- self.SkillMenu[Skill.Key]:addParam("LastHit", "Last Hit With This Skill", SCRIPT_PARAM_ONOFF, false)
                -- self.SkillMenu[Skill.Key]:addParam("MinManaMinions", "Minimum Mana %", SCRIPT_PARAM_SLICE, 0, 0, 100, 0)
        end
       
        --[[Configuration Menu ]]
        ConfigMenu = scriptConfig("Sida's Auto Carry: Configuration", "sidasacconfig")
        ConfigMenu:addParam("Focused", "Focus Selected Target", SCRIPT_PARAM_ONOFF, false)
        ConfigMenu:addParam("Butcher", "Butcher Mastery", SCRIPT_PARAM_SLICE, 0, 0, 2, 0)
        ConfigMenu:addParam("Spellblade", "Spellblade Mastery", SCRIPT_PARAM_ONOFF, true)
        ConfigMenu:addParam("Executioner", "Executioner Mastery", SCRIPT_PARAM_ONOFF, true)
        if VIP_USER then
                ConfigMenu:addParam("HitChance", "Ability Hitchance", SCRIPT_PARAM_SLICE, 60, 0, 100, 0)
        end
        ConfigMenu:addParam("Freeze", "Lane Freeze Toggle", SCRIPT_PARAM_ONKEYTOGGLE, false, 112)
 
        -- ConfigMenu:addParam("Boost", "Turbo Boost: Attack", SCRIPT_PARAM_SLICE, 0, 0, 1000, 0)
        -- ConfigMenu:addParam("AttackBoost", "Turbo Boost: Movement", SCRIPT_PARAM_SLICE, 0, 0, 1000, 0)
 
        --[[ Drawing Menu ]]
        DrawingMenu = scriptConfig("Sida's Auto Carry: Drawing", "sidasacdrawing")
        DrawingMenu:addParam("RangeCircle", "Champion Range Circle", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("RangeCircleColour", "Colour", SCRIPT_PARAM_COLOR, {255, 0, 189, 22})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("CastRangeCircle", "Spell Range Circle", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("CastRangeCircleColour", "Colour", SCRIPT_PARAM_COLOR, {183, 0, 26, 173})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("TargetCircle", "Circle Around Target", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("TargetCircleColour", "Colour", SCRIPT_PARAM_COLOR, {255, 0, 112, 95})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("MinionCircle", "Minion Marker With Prediction", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("MinionCircleColour", "Colour", SCRIPT_PARAM_COLOR, {183, 0, 26, 173})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("ArrowToTarget", "Line To Target", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("ArrowToTargetColour", "Colour", SCRIPT_PARAM_COLOR, {100, 255, 100, 100})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("StandZone", "Stand & Shoot Range", SCRIPT_PARAM_ONOFF, true)
        DrawingMenu:addParam("StandZoneColour", "Stand Zone Colour", SCRIPT_PARAM_COLOR, {183, 0, 26, 173})
        DrawingMenu:addParam("sep", "", SCRIPT_PARAM_INFO, "")
        DrawingMenu:addParam("LowFPS", "Use Low FPS Circles", SCRIPT_PARAM_ONOFF, false)
 
        --[[ Extras Menu ]]
        ExtrasMenu = scriptConfig("Sida's Auto Carry: Extras", "sidasacextras")
        ExtrasMenu:addParam("sep", "-- May Cause FPS Drops--", SCRIPT_PARAM_INFO, "")
        ExtrasMenu:addParam("ShowSwingTimer", "Show Swing Timer", SCRIPT_PARAM_ONOFF, true)
        ExtrasMenu:addParam("StandZone"..myHero.charName, "Stand Still And Shoot Range", SCRIPT_PARAM_SLICE, self:LoadStandRange(), 0, MyHero.TrueRange, 0)
 
        self:DisableAllModes()
        -- self.Boost = ConfigMenu.Boost
        -- self.AttackBoost = ConfigMenu.AttackBoost
end
 
function _MenuManager:OnTick()
        -- if self.Boost ~= ConfigMenu.Boost then
        --      print("Hyper Charge: Attack - When you start cancelling attacks, you've gone too high!")
        --      self.Boost = ConfigMenu.Boost
        -- end
 
        -- if self.AttackBoost ~= ConfigMenu.AttackBoost then
        --      print("Hyper Charge: Movement - When you stand still before each attack, you've gone too high!")
        --      self.AttackBoost = ConfigMenu.AttackBoost
        -- end
 
        if AutoCarryMenu.Active ~= self.AutoCarry and not self.AutoCarry then
                self:SetToggles(true, false, false, false)
        elseif MixedModeMenu.Active ~= self.MixedMode and not self.MixedMode then
                self:SetToggles(false, true, false, false)
        elseif LastHitMenu.Active ~= self.LastHit and not self.LastHit then
                self:SetToggles(false, false, true, false)
        elseif LaneClearMenu.Active ~= self.LaneClear and not self.LaneClear then
                self:SetToggles(false, false, false, true)
        end
 
        if ConfigMenu.Freeze then
                MixedModeMenu._param[3].text = "Mixed Mode         (Lane Freeze)"
                LastHitMenu._param[3].text = "Last Hit                 (Lane Freeze)"
        else
                MixedModeMenu._param[3].text = "Mixed Mode"
                LastHitMenu._param[3].text = "Last Hit"
        end
end
 
function _MenuManager:OnWndMsg(msg, key)
        if msg == WM_RBUTTONDOWN then
                local _Menu = self:GetActiveMenu()
                if _Menu and _Menu.Toggle then
                        self:DisableAllModes()
                end
        end
end
 
function _MenuManager:_OnUnload()
        self:SaveSkills()
        self:SaveStandRange()
end
 
function _MenuManager:_OnBugsplat()
        self:SaveSkills()
        self:SaveStandRange()
end
 
function _MenuManager:_OnExit()
        self:SaveSkills()
        self:SaveStandRange()
end
 
function _MenuManager:SetToggles(ac, mm, lh, lc)
        AutoCarryMenu.Active, self.AutoCarry = ac, ac
        MixedModeMenu.Active, self.MixedMode = mm, mm
        LastHitMenu.Active, self.LastHit = lh, lh
        LaneClearMenu.Active, self.LaneClear = lc, lc
end
 
function _MenuManager:DisableAllModes()
        AutoCarryMenu.Active = false
        MixedModeMenu.Active = false
        LastHitMenu.Active = false
        LaneClearMenu.Active = false
end
 
function _MenuManager:GetActiveMenu()
        if AutoCarryMenu.Active then
                return AutoCarryMenu
        elseif MixedModeMenu.Active then
                return MixedModeMenu
        elseif LastHitMenu.Active then
                return LastHitMenu
        elseif LaneClearMenu.Active then
                return LaneClearMenu
        end
end
 
function _MenuManager:SaveSkills()
        local _skills = {}
 
        for _, Skill in pairs(Skills.SkillsList) do
                if AutoCarryMenu[Skill.RawName.."AutoCarry"] ~= nil then
                        table.insert(_skills , {mode = "AutoCarry", name=Skill.RawName, value=AutoCarryMenu[Skill.RawName.."AutoCarry"]})
                end
                if MixedModeMenu[Skill.RawName.."MixedMode"] ~= nil then
                        table.insert(_skills , {mode = "MixedMode", name=Skill.RawName, value=MixedModeMenu[Skill.RawName.."MixedMode"]})
                end
                if LaneClearMenu[Skill.RawName.."LaneClear"] ~= nil then
                        table.insert(_skills , {mode = "LaneClear", name=Skill.RawName, value=LaneClearMenu[Skill.RawName.."LaneClear"]})
                end
        end
 
        local save = GetSave("SidasAutoCarry")
        save[myHero.charName] = _skills
        save:Save()
end
 
function _MenuManager:LoadSkill(Menu, Name, Mode)
        local save = GetSave("SidasAutoCarry")[myHero.charName]
        if not save then return false end
        for _, Skill in pairs(save) do
                if Skill.name == Name and Skill.mode == Mode then
                        return Skill.value
                end
        end
        return false
end
 
function _MenuManager:SaveStandRange()
        local save = GetSave("SidasAutoCarry")
 
        save.HoldZone = ExtrasMenu["StandZone"..myHero.charName]
        save:Save()
end
 
function _MenuManager:LoadStandRange()
        local save = GetSave("SidasAutoCarry")
 
        if save.HoldZone then
                return save.HoldZone
        else
                return 0
        end
end
 
--[[
                _Plugins Class
]]
 
class '_Plugins' Plugins = nil
 
function _Plugins:__init()
        self.Plugins = {}
        self.RegisteredBonusLastHitDamage = {}
        self.RegisteredPreAttack = {}
        Plugins = self
end
 
function _Plugins:RegisterPlugin(plugin, name)
        if plugin.OnTick then
                AddTickCallback(function() plugin:OnTick() end)
        end
        if plugin.OnDraw then
                AddDrawCallback(function() plugin:OnDraw() end)
        end
        if plugin.OnCreateObj then
                AddCreateObjCallback(function(obj) plugin:OnCreateObj(obj) end)
        end
        if plugin.OnDeleteObj then
                AddDeleteObjCallback(function(obj) plugin:OnDeleteObj(obj) end)
        end
        if plugin.OnLoad then
                plugin:OnLoad()
        end
        if plugin.OnUnload then
                AddUnloadCallback(function() plugin.OnUnload() end)
        end
        if plugin.OnWndMsg then
                AddMsgCallback(function(msg, key) plugin:OnWndMsg(msg, key) end)
        end
        if plugin.OnProcessSpell then
                AddProcessSpellCallback(function(unit, spell) plugin:OnProcessSpell(unit, spell) end)
        end
        if plugin.OnSendChat then
                AddChatCallback(function(text) plugin:OnSendChat(text) end)
        end
        if plugin.OnBugsplat then
                AddBugsplatCallback(function() plugin:OnBugsplat() end)
        end
        if plugin.OnAnimation then
                AddAnimationCallback(function(unit, anim) plugin:OnAnimation(unit, anim) end)
        end
        if plugin.OnSendPacket then
                AddSendPacketCallback(function(packet) plugin:OnSendPacket(packet) end)
        end
        if plugin.OnRecvPacket then
                AddRecvPacketCallback(function(packet) plugin:OnRecvPacket(packet) end)
        end
        if name then
                self.Plugins[name] = scriptConfig("Sida's Auto Carry Plugin: "..name, "sidasacautocarryplugin"..name)
                return self.Plugins[name]
        end
end
 
function _Plugins:RegisterBonusLastHitDamage(func)
        table.insert(self.RegisteredBonusLastHitDamage, func)
end
 
function _Plugins:RegisterPreAttack(func)
        table.insert(self.RegisteredPreAttack, func)
end
 
function _Plugins:RegisterOnAttacked(func)
        RegisterOnAttacked(func)
end
 
function _Plugins:GetProdiction(Key, Range, Speed, Delay, Width, Source, Callback)
        return ProdictManager.GetInstance():AddProdictionObject(Key, Range, Speed * 1000, Delay / 1000, Width, myHero, Callback)
end
 
--[[
                Drawing Class
]]
 
class '_Drawing'
 
function _Drawing:__init()
        AddDrawCallback(function() self:_OnDraw() end)
end
 
function _Drawing:_OnDraw()
        if DrawingMenu.RangeCircle then
                Helper:DrawCircleObject(myHero, MyHero.TrueRange, Helper:ArgbFromMenu(DrawingMenu.RangeCircleColour))
        end
 
        if DrawingMenu.CastRangeCircle and Crosshair.Skills_Crosshair.range ~= MyHero.TrueRange then
                Helper:DrawCircleObject(myHero, Crosshair.Skills_Crosshair.range, Helper:ArgbFromMenu(DrawingMenu.CastRangeCircleColour))
        end
 
        if DrawingMenu.TargetCircle and Crosshair.Target then
                Helper:DrawCircleObject(Crosshair.Target, 100, Helper:ArgbFromMenu(DrawingMenu.TargetCircleColour), 6)
        end
 
        if DrawingMenu.MinionCircle and Minions.KillableMinion then
                Helper:DrawCircleObject(Minions.KillableMinion, 80, Helper:ArgbFromMenu(DrawingMenu.MinionCircleColour), 6)
        end
 
        if DrawingMenu.ArrowToTarget and Crosshair.Target then
                self:DrawArrows(myHero, Crosshair.Target)
        end
 
        if DrawingMenu.StandZone and ExtrasMenu["StandZone"..myHero.charName] > 0 then
                Helper:DrawCircleObject(myHero, ExtrasMenu["StandZone"..myHero.charName], Helper:ArgbFromMenu(DrawingMenu.StandZoneColour))
        end
end
 
function _Drawing:DrawArrows(Start, End)
        if Start and End then
        DrawArrows(D3DXVECTOR3(Start.x, Start.y, Start.z), D3DXVECTOR3(End.x, End.y, End.z), 60, Helper:HexFromMenu(DrawingMenu.ArrowToTargetColour), 100)
    end
end
 
--[[
                _AutoUpdate Class
]]
 
class '_AutoUpdate' AutoUpdate = nil
 
function _AutoUpdate:__init()
        AutoUpdate = self
        self.CurrentVersion = 38
        self.Path = BOL_PATH.."Scripts\\Common\\RebornBetaVersion.beta"
 
        AddLoadCallback(function() self:_OnLoad() end)
end
 
function _AutoUpdate:_OnLoad()
        WriteFile("Revision="..self.CurrentVersion..";", self.Path)
end
 
_AutoUpdate()
 
--[[
                _Data Class
]]
 
 
class '_Data' Data = nil
 
 function _Data:__init()
        self.ResetSpells = {}
        self.SpellAttacks = {}
        self.NoneAttacks = {}
        self.ChampionData = {}
        self.MinionData = {}
        self.JungleData = {}
        self.ItemData = {}
        self.Skills = {}
        Data = self
 
        self:__GenerateNoneAttacks()
        self:__GenerateSpellAttacks()
        self:__GenerateResetSpells()
        self:_GenerateMinionData()
        self:_GenerateJungleData()
        self:_GenerateItemData()
        self:__GenerateChampionData()
        self:__GenerateSkillData()
 end
 
function _Data:__GenerateResetSpells()
        self:AddResetSpell("Powerfist")
        self:AddResetSpell("DariusNoxianTacticsONH")
        self:AddResetSpell("Takedown")
        self:AddResetSpell("Ricochet")
        self:AddResetSpell("BlindingDart")
        self:AddResetSpell("VayneTumble")
        self:AddResetSpell("JaxEmpowerTwo")
        self:AddResetSpell("MordekaiserMaceOfSpades")
        self:AddResetSpell("SiphoningStrikeNew")
        self:AddResetSpell("RengarQ")
        self:AddResetSpell("MonkeyKingDoubleAttack")
        self:AddResetSpell("YorickSpectral")
        self:AddResetSpell("ViE")
        self:AddResetSpell("GarenSlash3")
        self:AddResetSpell("HecarimRamp")
        self:AddResetSpell("XenZhaoComboTarget")
        self:AddResetSpell("LeonaShieldOfDaybreak")
        self:AddResetSpell("ShyvanaDoubleAttack")
        self:AddResetSpell("shyvanadoubleattackdragon")
        self:AddResetSpell("TalonNoxianDiplomacy")
        self:AddResetSpell("TrundleTrollSmash")
        self:AddResetSpell("VolibearQ")
        self:AddResetSpell("PoppyDevastatingBlow")
        self:AddResetSpell("SivirW")
end
 
function _Data:__GenerateSpellAttacks()
        self:AddSpellAttack("frostarrow")
        self:AddSpellAttack("CaitlynHeadshotMissile")
        self:AddSpellAttack("QuinnWEnhanced")
        self:AddSpellAttack("TrundleQ")
        self:AddSpellAttack("XenZhaoThrust")
        self:AddSpellAttack("XenZhaoThrust2")
        self:AddSpellAttack("XenZhaoThrust3")
        self:AddSpellAttack("GarenSlash2")
        self:AddSpellAttack("RenektonExecute")
        self:AddSpellAttack("RenektonSuperExecute")
        self:AddSpellAttack("KennenMegaProc")
end
 
function _Data:__GenerateNoneAttacks()
        self:AddNoneAttack("shyvanadoubleattackdragon")
        self:AddNoneAttack("ShyvanaDoubleAttack")
        self:AddNoneAttack("MonkeyKingDoubleAttack")
end
 
function _Data:_GenerateMinionData()
        self:AddMinionData((myHero.team == 100 and "Blue" or "Red").."_Minion_Basic", 400, 0)
        self:AddMinionData((myHero.team == 100 and "Blue" or "Red").."_Minion_Caster", 484, 0.68)
        self:AddMinionData((myHero.team == 100 and "Blue" or "Red").."_Minion_Wizard", 484, 0.68)
        self:AddMinionData((myHero.team == 100 and "Blue" or "Red").."_Minion_MechCannon", 365, 1.18)
        self:AddMinionData("obj_AI_Turret", 150, 1.14)
end
 
function _Data:_GenerateJungleData()
        self:AddJungleMonster("Worm12.1.1",             1)              -- Baron
        self:AddJungleMonster("Dragon6.1.1",            1)              -- Dragon
        self:AddJungleMonster("AncientGolem1.1.1",      1)              -- Blue Buff
        self:AddJungleMonster("AncientGolem7.1.1",      1)              -- Blue Buff
        self:AddJungleMonster("YoungLizard1.1.2",       2)              -- Blue Buff Add
        self:AddJungleMonster("YoungLizard7.1.3",       2)              -- Blue Buff Add
        self:AddJungleMonster("YoungLizard1.1.3",       2)              -- Blue Buff Add
        self:AddJungleMonster("YoungLizard7.1.2",       2)              -- Blue Buff Add
        self:AddJungleMonster("LizardElder4.1.1",       1)              -- Red Buff
        self:AddJungleMonster("LizardElder10.1.1",      1)              -- Red Buff
        self:AddJungleMonster("YoungLizard4.1.2",       2)              -- Red Buff Add
        self:AddJungleMonster("YoungLizard4.1.3",       2)              -- Red Buff Add
        self:AddJungleMonster("YoungLizard10.1.2",      2)              -- Red Buff Add
        self:AddJungleMonster("YoungLizard10.1.3",      2)              -- Red Buff Add
        self:AddJungleMonster("GiantWolf2.1.3",         1)              -- Big Wolf
        self:AddJungleMonster("GiantWolf8.1.3",         1)              -- Big Wolf
        self:AddJungleMonster("wolf2.1.1",                      2)              -- Small Wolf
        self:AddJungleMonster("wolf2.1.2",                      2)              -- Small Wolf
        self:AddJungleMonster("wolf8.1.1",                      2)              -- Small Wolf
        self:AddJungleMonster("wolf8.1.2",                      2)              -- Small Wolf
        self:AddJungleMonster("Wraith3.1.3",            1)              -- Big Wraith
        self:AddJungleMonster("Wraith9.1.3",            1)              -- Big Wraith
        self:AddJungleMonster("LesserWraith3.1.1",      2)              -- Small Wraith
        self:AddJungleMonster("LesserWraith3.1.2",      2)              -- Small Wraith
        self:AddJungleMonster("LesserWraith3.1.4",      2)              -- Small Wraith
        self:AddJungleMonster("LesserWraith9.1.1",      2)              -- Small Wraith
        self:AddJungleMonster("LesserWraith9.1.2",      2)              -- Small Wraith
        self:AddJungleMonster("LesserWraith9.1.4",      2)              -- Small Wraith
        self:AddJungleMonster("Golem5.1.2",             1)              -- Big Golem
        self:AddJungleMonster("Golem11.1.2",            1)              -- Big Golem
        self:AddJungleMonster("SmallGolem5.1.1",        2)              -- Small Golem
        self:AddJungleMonster("SmallGolem11.1.1",       2)              -- Small Golem
end
 
function _Data:_GenerateItemData()
        self:AddItemData("Blade of the Ruined King",    3153, true, 500)
        self:AddItemData("Bilgewater Cutlass",                  3144, true, 500)
        self:AddItemData("Deathfire Grasp",                     3128, true, 750)
        self:AddItemData("Hextech Gunblade",                    3146, true, 400)
        self:AddItemData("Blackfire Torch",                     3188, true, 750)
        self:AddItemData("Ravenous Hydra",                              3074, false)
        self:AddItemData("Sword of the Divine",                 3131, false)
        self:AddItemData("Tiamat",                                              3077, false)
        self:AddItemData("Entropy",                                     3184, false)
        self:AddItemData("Youmuu's Ghostblade",                 3142, false)
        self:AddItemData("Muramana",                                    3042, false)
end
 
ROLE_AD_CARRY = 1
ROLE_AP = 2
ROLE_SUPPORT = 3
ROLE_BRUISER = 4
ROLE_TANK = 5
 
function _Data:__GenerateChampionData()
                                                -- Champion, Projectile Speed,  GameplayCollisionRadius         Anti-bug delay                  Role
        self:AddChampionData("Aatrox",                  0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Ahri",            1.6,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Akali",           0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Alistar",         0,                              80,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Amumu",           0,                              55,                                             0,                      ROLE_TANK)
        self:AddChampionData("Anivia",          1.05,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Annie",           1,                              55,                                             0,                      ROLE_AP)
        self:AddChampionData("Ashe",            2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Blitzcrank",      0,                              80,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Brand",           1.975,                          65,                                             0,                      ROLE_AP)
        self:AddChampionData("Caitlyn",         2.5,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Cassiopeia",      1.22,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Chogath",         0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Corki",           2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Darius",                  0,                                      80,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Diana",           0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("DrMundo",         0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Draven",          1.4,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Elise",                   0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Evelynn",         0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Ezreal",          2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("FiddleSticks",    1.75,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Fiora",                   0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Fizz",            0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Galio",           0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Gangplank",               0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Garen",                   0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Gragas",          0,                              80,                                             0,                      ROLE_AP)
        self:AddChampionData("Graves",          3,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Hecarim",         0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Heimerdinger",    1.4,                            55,                                             0,                      ROLE_AP)
        self:AddChampionData("Irelia",                  0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Janna",           1.2,                            65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("JarvanIV",                0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Jax",                             0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Jayce",           2.2,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Jinx",            2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Karma",           1.2,                            65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Karthus",         1.25,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Kassadin",        0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Katarina",        0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Kayle",           1.8,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Kennen",          1.35,                           55,                                             0,                      ROLE_AP)
        self:AddChampionData("Khazix",                  0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("KogMaw",          1.8,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Leblanc",         1.7,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("LeeSin",                  0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Leona",           0,                              65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Lissandra",       0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("Lucian",          2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Lulu",            2.5,                            65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Lux",             1.55,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Malphite",        0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Malzahar",        1.5,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Maokai",          0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("MasterYi",        0,                              65,                                             0,                      ROLE_AP)
        self:AddChampionData("MissFortune",     2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("MonkeyKing",              0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Mordekaiser",     0,                              80,                                             0,                      ROLE_AP)
        self:AddChampionData("Morgana",         1.6,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Nami",            0,                              65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Nasus",           0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Nautilus",                0,                                      80,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Nidalee",         1.7,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Nocturne",                0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Nunu",            0,                              65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Olaf",                    0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Orianna",         1.4,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Pantheon",        0,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Poppy",                   0,                                      55,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Quinn",           1.85,                           65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Rammus",          0,                              65,                                             0,                      ROLE_TANK)
        self:AddChampionData("Renekton",                0,                                      80,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Rengar",                  0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Riven",                   0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Rumble",          0,                              80,                                             0,                      ROLE_AP)
        self:AddChampionData("Ryze",            2.4,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Sejuani",         0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Shaco",           0,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Shen",            0,                              65,                                             0,                      ROLE_TANK)
        self:AddChampionData("Shyvana",                 0,                                      50,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Singed",          0,                              65,                                             0,                      ROLE_TANK)
        self:AddChampionData("Sion",            0,                              80,                                             0,                      ROLE_AP)
        self:AddChampionData("Sivir",           1.4,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Skarner",         0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Sona",            1.6,                            65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Soraka",          1,                              65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Swain",           1.6,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Syndra",          1.2,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Talon",           0,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Taric",           0,                              65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Teemo",           1.3,                            55,                                             0,                      ROLE_AP)
        self:AddChampionData("Thresh",          0,                              55,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Tristana",        2.25,                           55,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Trundle",                 0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Tryndamere",              0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("TwistedFate",     1.5,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Twitch",          2.5,                            65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Udyr",                    0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Urgot",           1.3,                            80,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Varus",           2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Vayne",           2,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Veigar",          1.05,                           55,                                             0,                      ROLE_AP)
        self:AddChampionData("Vi",                              0,                                      50,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Viktor",          2.25,                           65,                                             0,                      ROLE_AP)
        self:AddChampionData("Vladimir",        1.4,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("Volibear",        0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Warwick",         0,                              65,                                             0,                      ROLE_TANK)
        self:AddChampionData("Xerath",          1.2,                            65,                                             0,                      ROLE_AP)
        self:AddChampionData("XinZhao",                 0,                                      65,                                             0,                              ROLE_BRUISER)
        self:AddChampionData("Yorick",          0,                              80,                                             0,                      ROLE_TANK)
        self:AddChampionData("Zac",             0,                              65,                                             0,                      ROLE_TANK)
        self:AddChampionData("Zed",             0,                              65,                                             0,                      ROLE_AD_CARRY)
        self:AddChampionData("Ziggs",           1.5,                            55,                                             0,                      ROLE_AP)
        self:AddChampionData("Zilean",          1.25,                           65,                                             0,                      ROLE_SUPPORT)
        self:AddChampionData("Zyra",            1.7,                            65,                                             0,                      ROLE_AP)
end                                                    
                                                       
--TODO: Complete the table
function _Data:__GenerateSkillData()
                                  --Name                          Enabled    Key         Range          Display Name                            Type                    MinMana  Reset   Require Attack Target     Speed                Delay   Width   Collision
        self:AddSkillData("Ezreal",                     true,    _Q,     1100,   "Q (Mystic Shot)",                     SPELL_LINEAR_COL,               0,       false,         false,                                  2,                      250,    70,             true)
        self:AddSkillData("Ezreal",                     true,    _W,     1050,   "W (Essence Flux)",            SPELL_LINEAR,                   0,       false,         false,                                  1.6,            250,    90,             false)
        self:AddSkillData("KogMaw",                     true,    _Q,     625,    "Q (Caustic Spittle)",         SPELL_TARGETED,                 0,       true,          true,                                   1.3,            260,    200,    false)
        self:AddSkillData("KogMaw",                     true,    _W,     625,    "W (Bio-Arcane Barrage)",      SPELL_SELF,                             0,       false,         false,                                  1.3,            260,    200,    false)
        self:AddSkillData("KogMaw",                     true,    _E,     850,    "E (Void Ooze)",                       SPELL_LINEAR,                   0,       false,         false,                                  1.3,            260,    200,    false)
        self:AddSkillData("KogMaw",                     true,    _R,     1700,   "R (Living Artillery)",        SPELL_LINEAR,                   0,       false,         false,                                  math.huge,      1000,   200,    false)
        self:AddSkillData("Sivir",                      true,    _Q,     1000,   "Q (Boomerang Blade)",         SPELL_LINEAR,                   0,       false,         false,                                  1.33,           250,    120,    false)
        self:AddSkillData("Sivir",                      true,    _W,     900,    "W (Ricochet)",                        SPELL_SELF,                             0,       true,          true,                                   1,                      0,              200,    false)
        self:AddSkillData("Graves",                     true,    _Q,     750,    "Q (Buck Shot)",                       SPELL_CONE,                             0,       false,         false,                                  2,                      250,    200,    false)
        self:AddSkillData("Graves",                     true,    _W,     700,    "W (Smoke Screen)",            SPELL_CIRCLE,                   0,       false,         false,                                  1400,           300,    500,    false)
        self:AddSkillData("Graves",                     true,    _E,     580,    "E (Quick Draw)",                      SPELL_SELF_AT_MOUSE,    0,       true,          true,                                   1450,           250,    200,    false)
        self:AddSkillData("Caitlyn",            true,    _Q,     1300,   "Q (Piltover Peacemaker)",     SPELL_LINEAR,                   0,       false,         false,                                  2.1,            625,    100,    true)
        self:AddSkillData("Corki",                      true,    _Q,     600,    "Q (Phosphorus Bomb)",         SPELL_CIRCLE,                   0,       false,         false,                                  2,                      200,    500,    false)
        self:AddSkillData("Corki",                      true,    _R,     1225,   "R (Missile Barrage)",         SPELL_LINEAR_COL,               0,       false,         false,                                  2,                      200,    50,             true)
        self:AddSkillData("Teemo",                      true,    _Q,     580,    "Q (Blinding Dart)",           SPELL_TARGETED,                 0,       false,         false,                                  2,                      0,              200,    false)
        self:AddSkillData("TwistedFate",        true,    _Q,     1200,   "Q (Wild Cards)",                      SPELL_LINEAR,                   0,       false,         false,                                  1.45,           250,    200,    false)
        self:AddSkillData("Vayne",                      true,    _Q,     750,    "Q (Tumble)",                          SPELL_SELF_AT_MOUSE,    0,       true,          true,                                   1.45,           250,    200,    false)
        self:AddSkillData("Vayne",                      true,    _R,     580,    "R (Final Hour)",                      SPELL_SELF,                             0,       false,         true,                                   1.45,           250,    200,    false)
        self:AddSkillData("MissFortune",        true,    _Q,     650,    "Q (Double Up)",                       SPELL_TARGETED,                 0,       true,          true,                                   1.45,           250,    200,    false)
        self:AddSkillData("MissFortune",        true,    _W,     580,    "W (Impure Shots)",            SPELL_SELF,                             0,       false,         true,                                   1.45,           250,    200,    false)
        self:AddSkillData("MissFortune",        true,    _E,     800,    "E (Make It Rain)",            SPELL_CIRCLE,                   0,       false,         false,                                  math.huge,      500,    500,    false)
        self:AddSkillData("Tristana",           true,    _Q,     580,    "Q (Rapid Fire)",                      SPELL_SELF,                             0,       false,         true,                                   1.45,           250,    200,    false)
        self:AddSkillData("Tristana",           true,    _E,     550,    "E (Explosive Shot)",          SPELL_TARGETED,                 0,       false,         false,                                  1.45,           250,    200,    false)
        self:AddSkillData("Draven",                     true,    _E,     950,    "E (Stand Aside)",                     SPELL_LINEAR,                   0,       false,         false,                                  1.37,           300,    130,    false)
        self:AddSkillData("Kennen",                     true,    _Q,     1050,   "Q (Thundering Shuriken)",     SPELL_LINEAR_COL,               0,       false,         false,                                  1.65,           180,    80,             true)
        self:AddSkillData("Ashe",                       true,    _W,     1200,   "W (Volley)",                          SPELL_LINEAR_COL,               0,       false,         false,                                  2,                      120,    85,             true)
        self:AddSkillData("Syndra",                     true,    _Q,     800,    "Q (Dark Sphere)",                     SPELL_CIRCLE,                   0,       false,         false,                                  math.huge,      400,    100,    false)
        self:AddSkillData("Jayce",                      true,    _Q,     1600,   "Q (Shock Blast)",                     SPELL_LINEAR_COL,               0,       false,         false,                                  2,                      350,    90,             true)
        self:AddSkillData("Nidalee",            true,    _Q,     1500,   "Q (Javelin Toss)",            SPELL_LINEAR_COL,               0,       false,         false,                                  1.3,            125,    80,             true)
        self:AddSkillData("Varus",                      true,    _E,     925,    "E (Hail of Arrows)",          SPELL_CIRCLE,                   0,       false,         false,                                  1.75,           240,    235,    false)
        self:AddSkillData("Quinn",                      true,    _Q,     1050,   "Q (Blinding Assault)",        SPELL_LINEAR_COL,               0,       false,         false,                                  1.55,           220,    90,             true)
        self:AddSkillData("LeeSin",                     true,    _Q,     975,    "Q (Sonic Wave)",                      SPELL_LINEAR_COL,               0,       false,         false,                                  1.5,            250,    70,             true)
        self:AddSkillData("Gangplank",          true,    _Q,     625,    "Q (Parley)",                          SPELL_TARGETED,                 0,       false,         false,                                  1.45,           250,    200,    false)
        self:AddSkillData("Twitch",                     true,    _W,     950,    "W (Venom Cask)",                      SPELL_CIRCLE,                   0,       false,         false,                                  1.4,            250,    275,    false)
        self:AddSkillData("Darius",                     true,    _W,     300,    "W (Crippling Strike)",        SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Hecarim",            true,    _Q,     300,    "Q (Rampage)",                         SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Warwick",            true,    _Q,     300,    "Q (Hungering Strike)",        SPELL_TARGETED,                 0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("MonkeyKing",         true,    _Q,     300,    "Q (Crushing Blow)",           SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Poppy",                      true,    _Q,     300,    "Q (Devastating Blow)",        SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Talon",                      true,    _Q,     300,    "Q (Noxian Diplomacy)",        SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Nautilus",           true,    _W,     300,    "W (Titans Wrath)",            SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Gangplank",          true,    _Q,     300,    "Q (Parlay)",                          SPELL_TARGETED,                 0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Vi",                         true,    _E,     300,    "E (Excessive Force)",         SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Rengar",                     true,    _Q,     300,    "Q (Savagery)",                        SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Trundle",            true,    _Q,     300,    "Q (Chomp)",                           SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Leona",                      true,    _Q,     300,    "Q (Shield Of Daybreak)",      SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Fiora",                      true,    _E,     300,    "E (Burst Of Speed)",          SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Blitzcrank",         true,    _E,     300,    "E (Power Fist)",                      SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Shyvana",            true,    _Q,     300,    "Q (Twin Blade)",                      SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Renekton",           true,    _W,     300,    "W (Ruthless Predator)",       SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Jax",                        true,    _W,     300,    "W (Empower)",                         SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("XinZhao",            true,    _Q,     300,    "Q (Three Talon Strike)",      SPELL_SELF,                             0,       true,          true,                                   2,                      0,              200,    true)
        self:AddSkillData("Nunu",                       true,    _E,     300,    "E (Snowball)",                        SPELL_TARGETED,                 0,       false,         false,                                  1.45,           250,    200,    false)
        self:AddSkillData("Khazix",                     true,    _Q,     300,    "Q (Taste Their Fear)",        SPELL_TARGETED,                 0,       true,          true,                                   1.45,           250,    200,    false)
        self:AddSkillData("Shen",                       true,    _Q,     300,    "Q (Vorpal Blade)",            SPELL_TARGETED,                 0,       false,         false,                                  1.45,           250,    200,    false)
end
 
function _Data:AddResetSpell(name)
        self.ResetSpells[name] = true
end
 
function _Data:AddSpellAttack(name)
        self.SpellAttacks[name] = true
end
 
function _Data:AddNoneAttack(name)
        self.NoneAttacks[name] = true
end
 
function _Data:AddChampionData(Champion, ProjSpeed, _GameplayCollisionRadius, Delay, _Priority)
        self.ChampionData[Champion] = {Name = Champion, ProjectileSpeed = ProjSpeed, GameplayCollisionRadius = _GameplayCollisionRadius, BugDelay = Delay and Delay or 0, Priority = _Priority }
end
 
function _Data:AddMinionData(Name, delay, ProjSpeed)
        self.MinionData[Name] = {Delay = delay, ProjectileSpeed = ProjSpeed}
end
 
function _Data:AddJungleMonster(Name, Priority)
        self.JungleData[Name] = Priority
end
 
function _Data:GetJunglePriority(Name)
        return self.JungleData[Name]
end
 
function _Data:AddItemData(Name, ID, RequiresTarget, Range)
        self.ItemData[ID] = _Item(Name, ID, RequiresTarget, Range)
end
 
function _Data:AddSkillData(Name, Enabled, Key, Range, DisplayName, Type, MinMana, AfterAttack, ReqAttackTarget, Speed, Delay, Width, Collision)
        if myHero.charName == Name then
                local skill = _Skill(Enabled, Key, Range, DisplayName, Type, MinMana, AfterAttack, ReqAttackTarget, Speed, Delay, Width, Collision)
                table.insert(self.Skills, skill)
        end
end
 
function _Data:GetProjectileSpeed(name)
        return self.ChampionData[name] and self.ChampionData[name].ProjectileSpeed or nil
end
 
function _Data:GetGameplayCollisionRadius(name)
        return self.ChampionData[name] and self.ChampionData[name].GameplayCollisionRadius or 65
end
 
function _Data:IsResetSpell(Spell)
        return self.ResetSpells[Spell.name]
end
 
function _Data:IsAttack(Spell)
        return (self.SpellAttacks[Spell.name] or Helper:StringContains(Spell.name, "attack")) and not self.NoneAttacks[Spell.name]
end
 
function _Data:IsJungleMinion(Object)
        return Object and Object.name and self.JungleData[Object.name] ~= nil
end
 
--[[ Initialize Classes ]]
function Init()
        if VIP_USER then
                if FileExist(SCRIPT_PATH..'Common/Prodiction.lua') then
                        require "Prodiction"
                else
                        print("[Reborn]: You need the Prodiction library from the forums!")
                        return false
                end
        end
        AutoCarry.Skills                = _Skills()
        AutoCarry.Keys                  = _Keys()
        AutoCarry.Items                 = _Items()
        AutoCarry.Data                  = _Data()
        AutoCarry.Jungle                = _Jungle()
        AutoCarry.Helper                = _Helper()
        AutoCarry.MyHero                = _MyHero()
        AutoCarry.Minions               = _Minions()
        AutoCarry.Crosshair     = _Crosshair(DAMAGE_PHYSICAL, MyHero.TrueRange, 0, false, false)
        AutoCarry.Orbwalker     = _Orbwalker()
        AutoCarry.Plugins               = _Plugins()
        Skills, Keys, Items, Data, Jungle, Helper, MyHero, Minions, Crosshair, Orbwalker = Helper:GetClasses()
        SwingTimer      = _SwingTimer()
        _MenuManager()
        _Drawing()
        _Structures()
        Streaming:CreateMenu()
        local _, files = ScanDirectory(BOL_PATH.."Scripts\\SidasAutoCarryPlugins")
        for _, file in pairs(files) do
                dofile(BOL_PATH.."Scripts\\SidasAutoCarryPlugins\\"..AutoCarry.Helper:TrimString(file))
        end
 
        PrintChat("<font color='#CCCCCC'> >> Valid license found! <<</font>")
        PrintChat("<font color='#CCCCCC'> >> Sida's Auto Carry - Reborn, loaded! <<</font>")
        PrintChat("<font color='#CCCCCC'> >> Loaded as "..(VIP_USER and "VIP" or "Non-VIP").." user <<</font>")
 
 
        --[[
                        Legacy Plugin Support
                        Plugins should be updated, this may be removed after a few months.
        ]]
 
        --AutoCarry.Orbwalker = AutoCarry.Crosshair.Attack_Crosshair
        AutoCarry.SkillsCrosshair = AutoCarry.Crosshair.Skills_Crosshair
        AutoCarry.CanMove = true
        AutoCarry.CanAttack = true
        AutoCarry.MainMenu = {}
        AutoCarry.PluginMenu = nil
        AutoCarry.EnemyTable = GetEnemyHeroes()
        AutoCarry.shotFired = false
        AutoCarry.OverrideCustomChampionSupport = false
        AutoCarry.CurrentlyShooting = false
        DoneInit = true
 
 
        class '_LegacyPlugin'
 
        function _LegacyPlugin:__init()
                AutoCarry.PluginMenu = scriptConfig("Sida's Auto Carry Plugin: "..myHero.charName, "sidasacplugin"..myHero.charName)
                require("SidasAutoCarryPlugin - "..myHero.charName)
                PrintChat(">> Sida's Auto Carry: Loaded "..myHero.charName.." plugin!")
                AddTickCallback(function() self:_OnTick() end)
 
                if PluginOnTick then
                        AddTickCallback(function() PluginOnTick() end)
                end
                if PluginOnDraw then
                        AddDrawCallback(function() PluginOnDraw() end)
                end
                if PluginOnCreateObj then
                        AddCreateObjCallback(function(obj) PluginOnCreateObj(obj) end)
                end
                if PluginOnDeleteObj then
                        AddDeleteObjCallback(function(obj) PluginOnDeleteObj(obj) end)
                end
                if PluginOnLoad then
                        PluginOnLoad()
                end
                if PluginOnUnload then
                        AddUnloadCallback(function() PluginOnUnload() end)
                end
                if PluginOnWndMsg then
                        AddMsgCallback(function(msg, key) PluginOnWndMsg(msg, key) end)
                end
                if PluginOnProcessSpell then
                        AddProcessSpellCallback(function(unit, spell) PluginOnProcessSpell(unit, spell) end)
                end
                if PluginOnSendChat then
                        AddChatCallback(function(text) PluginOnSendChat(text) end)
                end
                if PluginOnBugsplat then
                        AddBugsplatCallback(function() PluginOnBugsplat() end)
                end
                if PluginOnAnimation then
                        AddAnimationCallback(function(unit, anim) PluginOnAnimation(unit, anim) end)
                end
                if PluginOnSendPacket then
                        AddSendPacketCallback(function(packet) PluginOnSendPacket(packet) end)
                end
                if PluginOnRecvPacket then
                        AddRecvPacketCallback(function(packet) PluginOnRecvPacket(packet) end)
                end
                if PluginOnApplyParticle then
                        AddParticleCallback(function(unit, particle) PluginOnApplyParticle(unit, particle) end)
                end
                if OnAttacked then
                        RegisterOnAttacked(OnAttacked)
                end
                if PluginBonusLastHitDamage then
                        Plugins:RegisterBonusLastHitDamage(PluginBonusLastHitDamage)
                end
 
                if CustomAttackEnemy then
                        Plugins:RegisterPreAttack(CustomAttackEnemy)
                end
        end
 
        function _LegacyPlugin:_OnTick()
                AutoCarry.MainMenu.AutoCarry = AutoCarryMenu.Active
                AutoCarry.MainMenu.LastHit = LastHitMenu.Active
                AutoCarry.MainMenu.MixedMode = MixedModeMenu.Active
                AutoCarry.MainMenu.LaneClear = LaneClearMenu.Active
                MyHero:MovementEnabled(AutoCarry.CanMove)
                MyHero:AttacksEnabled(AutoCarry.CanAttack)
                if #AutoCarry.EnemyTable < #Helper.EnemyTable then
                        AutoCarry.EnemyTable = Helper.EnemyTable
                end
        end
 
        AutoCarry.GetAttackTarget = function(isCaster)
                return Crosshair:GetTarget()
        end
 
        AutoCarry.GetKillableMinion = function()
                return Minions.KillableMinion
        end
 
        AutoCarry.GetMinionTarget = function()
                return nil
        end
 
        AutoCarry.EnemyMinions = function()
                return Minions.EnemyMinions
        end
 
        AutoCarry.AllyMinions = function()
                return Minions.AllyMinions
        end
 
        AutoCarry.GetJungleMobs = function()
                return Jungle.JungleMonsters
        end
 
        AutoCarry.GetLastAttacked = function()
                return Orbwalker.LastEnemyAttacked
        end
 
        AutoCarry.GetNextAttackTime = function()
                return Orbwalker:GetNextAttackTime()
        end
 
        AutoCarry.CastSkillshot = function (skill, target)
            if VIP_USER then
                pred = TargetPredictionVIP(skill.range, skill.speed*1000, (skill.delay/1000 - (GetLatency()/2)/1000), skill.width)
            elseif not VIP_USER then
                pred = TargetPrediction(skill.range, skill.speed, skill.delay, skill.width)
            end
            local predPos = pred:GetPrediction(target)
            if predPos and GetDistance(predPos) <= skill.range then
                if VIP_USER and pred:GetHitChance(target) > ConfigMenu.HitChance/100 then --TODO
                    if not skill.minions or not AutoCarry.GetCollision(skill, myHero, predPos) then
                       CastSpell(skill.spellKey, predPos.x, predPos.z)
                    end
                elseif not VIP_USER then
                    if not skill.minions or not AutoCarry.GetCollision(skill, myHero, predPos) then
                            CastSpell(skill.spellKey, predPos.x, predPos.z)
                    end
                end
            end
        end
 
        AutoCarry.GetCollision = function (skill, source, destination)
                if VIP_USER then
                        local col = Collision(skill.range, skill.speed*1000 , (skill.delay/1000 - (GetLatency()/2)/1000), skill.width)
                        return col:GetMinionCollision(source, destination)
                else
                        return willHitMinion(destination, skill.width)
                end
        end
 
        AutoCarry.GetPrediction = function(skill, target)
                if VIP_USER then
                        pred = TargetPredictionVIP(skill.range, skill.speed*1000, skill.delay/1000, skill.width)
                elseif not VIP_USER then
                        pred = TargetPrediction(skill.range, skill.speed, skill.delay, skill.width)
                end
                return pred:GetPrediction(target)
        end
 
        AutoCarry.IsValidHitChance = function(skill, target)
                return true
        end
 
        AutoCarry.GetProdiction = function(Key, Range, Speed, Delay, Width, Source, Callback)
                return AutoCarry.Plugins:GetProdiction(Key, Range, Speed, Delay, Width, Source, Callback)
        end
 
        function willHitMinion(predic, width)
                for _, minion in pairs(Minions.EnemyMinions.objects) do
                        if minion ~= nil and minion.valid and string.find(minion.name,"Minion_") == 1 and minion.team ~= player.team and minion.dead == false then
                                if predic ~= nil then
                                        ex = player.x
                                        ez = player.z
                                        tx = predic.x
                                        tz = predic.z
                                        dx = ex - tx
                                        dz = ez - tz
                                        if dx ~= 0 then
                                                m = dz/dx
                                                c = ez - m*ex
                                        end
                                        mx = minion.x
                                        mz = minion.z
                                        distanc = (math.abs(mz - m*mx - c))/(math.sqrt(m*m+1))
                                        if distanc < width and math.sqrt((tx - ex)*(tx - ex) + (tz - ez)*(tz - ez)) > math.sqrt((tx - mx)*(tx - mx) + (tz - mz)*(tz - mz)) then
                                                return true
                                        end
                                end
                        end
                end
                return false
        end
 
        if FileExist(LIB_PATH .."SidasAutoCarryPlugin - "..myHero.charName..".lua") then
            _LegacyPlugin()
        end
end
 
function OnLoad()
end

--UPDATEURL=
--HASH=8748F606EB1322D0B2072F185AA8AA8A
