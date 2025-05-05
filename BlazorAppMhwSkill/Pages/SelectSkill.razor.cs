using BlazorAppMhwSkill.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using System.Text;
using Microsoft.JSInterop;
using BlazorAppMhwSkill.Logic;
using System.Net.Sockets;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using System.Text.Unicode;

namespace BlazorAppMhwSkill.Pages
{
    public partial class SelectSkill : ComponentBase
    {
    //
        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        protected HttpClient Http { get; set; } = default!;

        [Parameter]
        public string Title { get; set; } = "欲しいスキルになる防具の組み合わせ";

        // シリーズスキル　ドロップダウン用
        private List<string> seriesSkills = new List<string>();

        // グループスキル　ドロップダウン用
        private List<string> groupSkills = new List<string>();

        // スキル　ドロップダウン用
        private List<Skill> skills = new List<Skill>();

        // 護石　絞込用
        private List<Goseki> gosekis = new List<Goseki>();

        private List<SkillSearchResult> skillSearchResults = new List<SkillSearchResult>();

        private string searchCondStr = string.Empty;

        // シリーズスキル数　ラジオボタン用
        private string? selectedSeriesSkillCount = "1";

        // ドロップダウンで選択されたシリーズスキル１
        private string? selectedSeriesSkill;

        // ドロップダウンで選択されたシリーズスキル２
        private string? selectedSeriesSkill2;

        // ドロップダウンで選択されたグループスキル
        private string? selectedGroupSkill;

        // ラジオボタン初期選択用
        private bool checkedSeriesSkillCount1 = false;
        private bool checkedSeriesSkillCount2 = false;

        // 耐性の条件
        private int requiredFire = -20;
        private int requiredWater = -20;
        private int requiredThunder = -20;
        private int requiredIce = -20;
        private int requiredDragon = -20;


        /// <summary>
        /// シリーズスキル数ラジオボタン選択切り替え時に発生するイベント
        /// 選択結果を変数に格納する
        /// </summary>
        /// <param name="e"></param>
        private void OnModeChanged(ChangeEventArgs e)
        {
            selectedSeriesSkillCount = e.Value?.ToString() ?? "";

            if(selectedSeriesSkillCount == "1")
            {
                checkedSeriesSkillCount1 = true;
                checkedSeriesSkillCount2 = false;
            }
            else
            {
                checkedSeriesSkillCount1 = false;
                checkedSeriesSkillCount2 = true;

            }

        }


        private List<CondSkill> condSkills = new();

        private void AddItem()
        {
            condSkills.Add(new CondSkill());
        }

        private void RemoveItem(CondSkill item)
        {
            condSkills.Remove(item);
        }

        private void PrintItems()
        {
            foreach (var item in condSkills)
            {
                Console.WriteLine($"名前: {item.SkillName}, 年齢: {item.SkillLevel}");
            }
        }

        private List<Armor> armors = new List<Armor>();

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var tempArmors = await Http.GetFromJsonAsync<Armor[]>($"json/armor.json?nocache={DateTime.Now.Ticks}");
            if (tempArmors == null)
            {
                return;
            }
            armors = tempArmors.ToList();

            var tempGosekis = await Http.GetFromJsonAsync<Goseki[]>($"json/goseki.json?nocache={DateTime.Now.Ticks}");
            if (tempGosekis == null)
            {
                return;
            }

            gosekis = tempGosekis.ToList();

            var tempSkills = await Http.GetFromJsonAsync<Skill[]>($"json/skill.json?nocache={DateTime.Now.Ticks}");
            if (tempSkills == null)
            {
                return;
            }

            skills = tempSkills.ToList();


            // 防具一覧からシリーズスキルを重複を排除して取得
            seriesSkills = armors.Where(x => !string.IsNullOrWhiteSpace(x.SeriesSkill)).GroupBy(x => x.SeriesSkill).Select(x => x.Key).ToList();

            // 防具一覧からグループスキルを重複を排除して取得
            groupSkills = armors.Where(x => !string.IsNullOrWhiteSpace(x.GroupSkill)).GroupBy(x => x.GroupSkill).Select(x => x.Key).ToList();

            this.checkedSeriesSkillCount1 = true;
            this.checkedSeriesSkillCount2 = false;
        }


        private void Search()
        {
            if (string.IsNullOrEmpty(selectedSeriesSkill))
            {
                return;
            }

            if (checkedSeriesSkillCount2 && string.IsNullOrEmpty(selectedSeriesSkill2))
            {
                return;
            }

            int resultCount = 0;
            bool isAllBreak = false;

            skillSearchResults = new List<SkillSearchResult>();
            // 検索条件をTSV出力用の文字列にする
            StringBuilder serchCondString = new StringBuilder();
            serchCondString.AppendLine("●検索条件");

            serchCondString.Append("【シリーズスキル】");
            serchCondString.Append(selectedSeriesSkill);
            if (checkedSeriesSkillCount2)
            {
                serchCondString.Append(",");
                serchCondString.AppendLine(selectedSeriesSkill2);
            }

            if (!string.IsNullOrEmpty(selectedGroupSkill))
            {
                serchCondString.Append("【グループスキル】");
                serchCondString.AppendLine(selectedGroupSkill);
            }

            serchCondString.AppendLine("【スキル】");
            foreach (var condSkill in condSkills)
            {
                condSkill.Slot = skills.Where(r => r.SkillName == condSkill.SkillName).First().Slot;

                serchCondString.AppendLine($"{condSkill.SkillName}:{condSkill.SkillLevel}");
            }

            serchCondString.AppendLine($"【耐性】火：{requiredFire}、水：{requiredWater}、雷：{requiredThunder}、氷：{requiredIce}、龍：{requiredDragon}");
            serchCondString.AppendLine("●検索結果");
            this.searchCondStr = serchCondString.ToString();

            //bool isSelect2SeriesSkill = checkedSeriesSkillCount2;

            foreach (var head in this.armors.Where(r => r.Part == 1))
            {
                bool useSonota = false;
                List<string> usedSeriesSkills = new List<string>();
                usedSeriesSkills.Add(head.SeriesSkill);

                List<string> allowSeriesSkills = new List<string>();
                // シリーズスキル２種で頭が違う場合、選択したシリーズ以外は許可されない
                if (checkedSeriesSkillCount1 && head.SeriesSkill != this.selectedSeriesSkill)
                {
                    allowSeriesSkills.Add(this.selectedSeriesSkill);
                    useSonota = true;
                }
                // シリーズスキル２種で頭がどっちとも違う場合、選択したどっちかしか許可されない
                else if (checkedSeriesSkillCount2 && head.SeriesSkill != this.selectedSeriesSkill && head.SeriesSkill != this.selectedSeriesSkill2)
                {
                    allowSeriesSkills.Add(this.selectedSeriesSkill);
                    allowSeriesSkills.Add(this.selectedSeriesSkill2);
                    useSonota = true;
                }

                foreach (var body in this.armors.Where(r => r.Part == 2 && (allowSeriesSkills.Count == 0 || allowSeriesSkills.Contains(r.SeriesSkill))))
                {
                    List<string> bodyUsedSkills = new List<string>(usedSeriesSkills);
                    List<string> bodyAllowSeriesSkills = new List<string>(allowSeriesSkills);

                    //if (!bodyUsedSkills.Contains(body.SeriesSkill))
                    //{
                        bodyUsedSkills.Add(body.SeriesSkill);
                    //}

                    // あと１種の場合はこれ以上絞らない
                    if (bodyAllowSeriesSkills.Count == 1)
                    {
                    }
                    // あと２種の場合１種にしぼれるか検証
                    else if (bodyAllowSeriesSkills.Count == 2)
                    {
                        // 体と頭で同じシリーズスキルを選んだ場合、そのスキルはもう使えない
                        if (body.SeriesSkill == head.SeriesSkill)
                        {
                            bodyAllowSeriesSkills.Remove(body.SeriesSkill);
                        }
                    }
                    // まだ絞ってない場合絞れるか検証
                    else
                    {
                        // シリーズスキル１種で体が違えば
                        if (checkedSeriesSkillCount1 && body.SeriesSkill != this.selectedSeriesSkill)
                        {
                            bodyAllowSeriesSkills.Add(this.selectedSeriesSkill);
                            useSonota = true;
                        }
                        // シリーズスキル２種
                        else if (checkedSeriesSkillCount2)
                        {
                            // 体がどっちとも違う場合、選択したどっちかしか許可されない
                            if (body.SeriesSkill != this.selectedSeriesSkill && body.SeriesSkill != this.selectedSeriesSkill2)
                            {
                                bodyAllowSeriesSkills.Add(this.selectedSeriesSkill);
                                bodyAllowSeriesSkills.Add(this.selectedSeriesSkill2);
                                useSonota = true;
                            }
                            //// 体と頭で同じシリーズスキルを選んだ場合、まだその他のスキルを選べる
                            //else if (body.SeriesSkill == head.SeriesSkill)
                            //{
                            //    if (body.SeriesSkill == this.selectedSeriesSkill)
                            //    {
                            //        bodyAllowSeriesSkills.Add(this.selectedSeriesSkill2);
                            //    }
                            //    else
                            //    {
                            //        bodyAllowSeriesSkills.Add(this.selectedSeriesSkill);
                            //    }

                            //}

                        }

                    }

                    foreach (var arm in this.armors.Where(r => r.Part == 3 && (bodyAllowSeriesSkills.Count == 0 || bodyAllowSeriesSkills.Contains(r.SeriesSkill))))
                    {
                        List<string> armUsedSkills = new List<string>(bodyUsedSkills);
                        List<string> armAllowSeriesSkills = new List<string>(bodyAllowSeriesSkills);

                        //if (!armUsedSkills.Contains(arm.SeriesSkill))
                        //{
                            armUsedSkills.Add(arm.SeriesSkill);
                        //}

                        // あと１種の場合はこれ以上絞らない
                        if (armAllowSeriesSkills.Count == 1)
                        {
                        }
                        // あと２種の場合１種にしぼれるか検証
                        else if (armAllowSeriesSkills.Count == 2)
                        {
                            if (armUsedSkills.Where(r => r == selectedSeriesSkill).Count() >= 2)
                            {
                                armAllowSeriesSkills.Remove(selectedSeriesSkill);
                            }
                            else if (armUsedSkills.Where(r => r == selectedSeriesSkill2).Count() >= 2)
                            {
                                armAllowSeriesSkills.Remove(selectedSeriesSkill2);
                            }

                        }
                        // まだ絞ってない場合絞れるか検証
                        else
                        {

                            // シリーズスキル１種で腕が違えば
                            if (checkedSeriesSkillCount1 && arm.SeriesSkill != this.selectedSeriesSkill)
                            {
                                armAllowSeriesSkills.Add(this.selectedSeriesSkill);
                                useSonota = true;
                            }
                            // シリーズスキル２種
                            else if (checkedSeriesSkillCount2)
                            {
                                // 腕がどっちとも違う場合
                                if (arm.SeriesSkill != this.selectedSeriesSkill && arm.SeriesSkill != this.selectedSeriesSkill2)
                                {
                                    useSonota = true;
                                    // シリーズスキル１が既に２回使われている場合、シリーズ２のみ許可
                                    if (armUsedSkills.Where(r => r == selectedSeriesSkill).Count() >= 2)
                                    {
                                        armAllowSeriesSkills.Add(selectedSeriesSkill2);
                                    }
                                    // シリーズスキル２が既に２回使われている場合、シリーズ１のみ許可
                                    else if (armUsedSkills.Where(r => r == selectedSeriesSkill2).Count() >= 2)
                                    {
                                        armAllowSeriesSkills.Add(selectedSeriesSkill);
                                    }
                                    // その他の場合は選択したどっちかしか許可されない
                                    else
                                    {
                                        armAllowSeriesSkills.Add(this.selectedSeriesSkill);
                                        armAllowSeriesSkills.Add(this.selectedSeriesSkill2);
                                    }

                                }
                                // シリーズ１が３回の場合はシリーズ２のみ許可
                                else if (armUsedSkills.Where(r => r == selectedSeriesSkill).Count() >= 3)
                                {
                                    armAllowSeriesSkills.Add(selectedSeriesSkill2);
                                    useSonota = true;
                                }
                                // シリーズ２が３回の場合はシリーズ１のみ許可
                                else if (armUsedSkills.Where(r => r == selectedSeriesSkill2).Count() >= 3)
                                {
                                    armAllowSeriesSkills.Add(selectedSeriesSkill);
                                    useSonota = true;
                                }

                            }
                        }

                        foreach (var weist in this.armors.Where(r => r.Part == 4 && (armAllowSeriesSkills.Count == 0 || armAllowSeriesSkills.Contains(r.SeriesSkill))))
                        {
                            List<string> weistUsedSeriesSkill = new List<string>(armUsedSkills);
                            List<string> weistAllowSeriesSkills = new List<string>(armAllowSeriesSkills);


                            //if (!weistUsedSeriesSkill.Contains(weist.SeriesSkill))
                            //{
                                weistUsedSeriesSkill.Add(weist.SeriesSkill);
                            //}

                            // あと１種の場合はこれ以上絞らない
                            if (weistAllowSeriesSkills.Count == 1)
                            {
                            }
                            // あと２種の場合１種にしぼれるか検証
                            else if (weistAllowSeriesSkills.Count == 2)
                            {
                                if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill).Count() >= 2)
                                {
                                    weistAllowSeriesSkills.Remove(selectedSeriesSkill);
                                }
                                else if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill2).Count() >= 2)
                                {
                                    weistAllowSeriesSkills.Remove(selectedSeriesSkill2);
                                }
                            }
                            // まだ絞ってない場合絞れるか検証
                            else
                            {

                                // シリーズスキル１種で胴が違えば
                                if (checkedSeriesSkillCount1 && weist.SeriesSkill != this.selectedSeriesSkill)
                                {
                                    weistAllowSeriesSkills.Add(this.selectedSeriesSkill);
                                    useSonota = true;
                                }
                                // シリーズスキル２種
                                else if (checkedSeriesSkillCount2)
                                {

                                    // 胴がどっちとも違う場合
                                    if (weist.SeriesSkill != this.selectedSeriesSkill && weist.SeriesSkill != this.selectedSeriesSkill2)
                                    {
                                        // シリーズスキル１が既に２回使われている場合、シリーズ２のみ許可
                                        if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill).Count() >= 2)
                                        {
                                            weistAllowSeriesSkills.Add(selectedSeriesSkill2);
                                        }
                                        // シリーズスキル２が既に２回使われている場合、シリーズ１のみ許可
                                        else if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill2).Count() >= 2)
                                        {
                                            weistAllowSeriesSkills.Add(selectedSeriesSkill);
                                        }
                                        // その他の場合は選択したどっちかしか許可されない
                                        else
                                        {
                                            weistAllowSeriesSkills.Add(this.selectedSeriesSkill);
                                            weistAllowSeriesSkills.Add(this.selectedSeriesSkill2);
                                        }

                                        useSonota = true;
                                    }
                                    // シリーズ１が３回の場合はシリーズ２のみ許可
                                    else if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill).Count() >= 3)
                                    {
                                        weistAllowSeriesSkills.Add(selectedSeriesSkill2);
                                        useSonota = true;
                                    }
                                    // シリーズ２が３回の場合はシリーズ１のみ許可
                                    else if (weistUsedSeriesSkill.Where(r => r == selectedSeriesSkill2).Count() >= 3)
                                    {
                                        weistAllowSeriesSkills.Add(selectedSeriesSkill);
                                        useSonota = true;
                                    }

                                }
                            }

                            foreach (var foot in this.armors.Where(r => r.Part == 5 && (weistAllowSeriesSkills.Count == 0 || weistAllowSeriesSkills.Contains(r.SeriesSkill))))
                            {
                                // 属性耐性での絞込
                                if (requiredFire > head.Fire + body.Fire + arm.Fire + weist.Fire + foot.Fire) continue;
                                if (requiredWater > head.Water + body.Water + arm.Water + weist.Water + foot.Water) continue;
                                if (requiredThunder > head.Thunder + body.Thunder + arm.Thunder + weist.Thunder + foot.Thunder) continue;
                                if (requiredIce > head.Ice + body.Ice + arm.Ice + weist.Ice + foot.Ice) continue;
                                if (requiredDragon > head.Dragon + body.Dragon + arm.Dragon + weist.Dragon + foot.Dragon) continue;

                                
                                // グループスキルでの絞込
                                if (!string.IsNullOrEmpty(selectedGroupSkill))
                                {
                                    int groupMatchCount = 0;
                                    if (head.GroupSkill == selectedGroupSkill) groupMatchCount++;
                                    if (body.GroupSkill == selectedGroupSkill) groupMatchCount++;
                                    if (arm.GroupSkill == selectedGroupSkill) groupMatchCount++;
                                    if (weist.GroupSkill == selectedGroupSkill) groupMatchCount++;
                                    if (foot.GroupSkill == selectedGroupSkill) groupMatchCount++;

                                    // 一致するグループが3つ未満ならNG
                                    if (groupMatchCount < 3)
                                    {
                                        continue;
                                    }
                                }

                                // 防具のスキルをスキル名で集約してcanUseSkillsに入れる
                                List<CondSkill> canUseSkills = new List<CondSkill>();
                                foreach (var skill in head.Skills)
                                {
                                    canUseSkills.Add(new CondSkill()
                                {
                                    SkillName = skill.Name,
                                    SkillLevel = skill.Level,
                                    Slot = skills.Where(r=>r.SkillName== skill.Name).First().Slot
                                    });
                                }
                                foreach (var skill in body.Skills)
                                {
                                    var tempSkill = canUseSkills.Where(r => r.SkillName == skill.Name).FirstOrDefault();

                                    if(tempSkill == null)
                                    {
                                        canUseSkills.Add(new CondSkill()
                                        {
                                            SkillName = skill.Name,
                                            SkillLevel = skill.Level,
                                            Slot = skills.Where(r => r.SkillName == skill.Name).First().Slot
                                        });
                                    }
                                    else
                                    {
                                        tempSkill.SkillLevel += skill.Level;
                                    }
                                }
                                foreach (var skill in arm.Skills)
                                {
                                    var tempSkill = canUseSkills.Where(r => r.SkillName == skill.Name).FirstOrDefault();

                                    if (tempSkill == null)
                                    {
                                        canUseSkills.Add(new CondSkill()
                                        {
                                            SkillName = skill.Name,
                                            SkillLevel = skill.Level,
                                            Slot = skills.Where(r => r.SkillName == skill.Name).First().Slot
                                        });
                                    }
                                    else
                                    {
                                        tempSkill.SkillLevel += skill.Level;
                                    }
                                }
                                foreach (var skill in weist.Skills)
                                {
                                    var tempSkill = canUseSkills.Where(r => r.SkillName == skill.Name).FirstOrDefault();

                                    if (tempSkill == null)
                                    {
                                        canUseSkills.Add(new CondSkill()
                                        {
                                            SkillName = skill.Name,
                                            SkillLevel = skill.Level,
                                            Slot = skills.Where(r => r.SkillName == skill.Name).First().Slot
                                        });
                                    }
                                    else
                                    {
                                        tempSkill.SkillLevel += skill.Level;
                                    }
                                }
                                foreach (var skill in foot.Skills)
                                {
                                    var tempSkill = canUseSkills.Where(r => r.SkillName == skill.Name).FirstOrDefault();

                                    if (tempSkill == null)
                                    {
                                        canUseSkills.Add(new CondSkill()
                                        {
                                            SkillName = skill.Name,
                                            SkillLevel = skill.Level,
                                            Slot = skills.Where(r => r.SkillName == skill.Name).First().Slot
                                        });
                                    }
                                    else
                                    {
                                        tempSkill.SkillLevel += skill.Level;
                                    }
                                }

                                List<int> slots = new List<int>();
                                slots.Add(head.Slot1);
                                slots.Add(head.Slot2);
                                slots.Add(head.Slot3);
                                slots.Add(body.Slot1);
                                slots.Add(body.Slot2);
                                slots.Add(body.Slot3);
                                slots.Add(arm.Slot1);
                                slots.Add(arm.Slot2);
                                slots.Add(arm.Slot3);
                                slots.Add(weist.Slot1);
                                slots.Add(weist.Slot2);
                                slots.Add(weist.Slot3);
                                slots.Add(foot.Slot1);
                                slots.Add(foot.Slot2);
                                slots.Add(foot.Slot3);

                                int slot3 = slots.Count(r => r == 3);
                                int slot2 = slots.Count(r => r == 2);
                                int slot1 = slots.Count(r => r == 1);

                                //int slot3 = head.Slot3
                                //    + body.Slot3
                                //    + arm.Slot3
                                //    + weist.Slot3
                                //    + foot.Slot3;

                                //int slot2 = head.Slot2
                                //    + body.Slot2
                                //    + arm.Slot2
                                //    + weist.Slot2
                                //    + foot.Slot2;

                                //int slot1 = head.Slot1
                                //    + body.Slot1
                                //    + arm.Slot1
                                //    + weist.Slot1
                                //    + foot.Slot1;

                                List<CondSkill> addSkills = new List<CondSkill>();
                                // 条件に設定したスキルが全部達成できるか調べる
                                // スロットの降順で調べる（下のレベルは上のレベルで代用できるから）
                                bool isNg = false;
                                bool useGoseki = false;
                                string usedGoseki = string.Empty;
                                foreach (var needsSkill in condSkills.OrderByDescending(r=>r.Slot).ThenBy(r => r.SkillLevel))
                                {
                                    int needsLevel = needsSkill.SkillLevel;

                                    var useSkill = canUseSkills.Where(r =>
                                            r.SkillName == needsSkill.SkillName
                                            && r.SkillLevel > 0).FirstOrDefault();
                                    if (useSkill != null)
                                    {
                                        // 必要レベルに達していない場合
                                        if(needsLevel > useSkill.SkillLevel)
                                        {
                                            // 必要レベルを減らして続行
                                            needsLevel -= useSkill.SkillLevel;
                                            useSkill.SkillLevel = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            // 使用可能レベルを減らして達成。次のスキルへ
                                            useSkill.SkillLevel -= needsLevel;
                                            continue;
                                        }
                                    }

                                    // slotで達成できるか調査
                                    if(needsSkill.Slot == 3)
                                    {
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot3)
                                        {
                                            if (slot3 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot3
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot3;
                                            slot3 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot3 -= needsLevel;
                                            continue;
                                        }
                                    }
                                    else if (needsSkill.Slot == 2)
                                    {
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot2)
                                        {
                                            if (slot2 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot2
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot2;
                                            slot2 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot2 -= needsLevel;
                                            continue;
                                        }

                                        // slot3で代用
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot3)
                                        {
                                            if (slot3 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot3
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot3;
                                            slot3 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot3 -= needsLevel;
                                            continue;
                                        }

                                    }
                                    else if (needsSkill.Slot == 1)
                                    {
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot1)
                                        {
                                            if (slot1 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot1
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot1;
                                            slot1 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot1 -= needsLevel;
                                            continue;
                                        }

                                        // slot2で代用
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot2)
                                        {
                                            if (slot2 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot2
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot2;
                                            slot2 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot2 -= needsLevel;
                                            continue;
                                        }

                                        // slot3で代用
                                        // スロットの空きが足りない場合
                                        if (needsLevel > slot3)
                                        {
                                            if (slot3 > 0)
                                            {
                                                addSkills.Add(new CondSkill()
                                                {
                                                    SkillName = needsSkill.SkillName,
                                                    SkillLevel = slot3
                                                });
                                            }
                                            // 必要レベルを減らして続行
                                            needsLevel -= slot3;
                                            slot3 = 0;
                                        }
                                        // 必要レベルが足りている場合
                                        else
                                        {
                                            addSkills.Add(new CondSkill()
                                            {
                                                SkillName = needsSkill.SkillName,
                                                SkillLevel = needsLevel
                                            });
                                            // 使用可能スロットを減らして達成。次のスキルへ
                                            slot3 -= needsLevel;
                                            continue;
                                        }
                                    }

                                    // 護石使用済みの場合はNG
                                    if (useGoseki)
                                    {
                                        isNg = true;
                                        break;
                                    }

                                    // 護石でできるか調べる
                                    var goseki = gosekis.Where(r => r.SkillName == needsSkill.SkillName && r.Level >= needsLevel).OrderBy(r=>r.Level).FirstOrDefault();
                                    if (goseki != null)
                                    {
                                        useGoseki = true;
                                        usedGoseki = goseki.GosekiName;
                                    }
                                    else
                                    {
                                        // 使える護石がなければNG
                                        isNg = true;
                                        break;
                                    }

                                }

                                if (isNg)
                                {
                                    continue;
                                }


                                SkillSearchResult skillSearchResult = new SkillSearchResult();
                                List<string> listGrp = new List<string>();
                                listGrp.Add(head.GroupSkill);
                                listGrp.Add(body.GroupSkill);
                                listGrp.Add(arm.GroupSkill);
                                listGrp.Add(weist.GroupSkill);
                                listGrp.Add(foot.GroupSkill);
                                var grp = listGrp.GroupBy(r => r).Where(g => g.Count() >= 3).Select(k => k.Key).FirstOrDefault();

                                if (!string.IsNullOrEmpty(grp))
                                {
                                    skillSearchResult.GroupSkill = grp;
                                }
                                skillSearchResult.Head = head.Name;
                                skillSearchResult.Body = body.Name;
                                skillSearchResult.Arm = arm.Name;
                                skillSearchResult.Weist = weist.Name;
                                skillSearchResult.Foot = foot.Name;
                                skillSearchResult.Slot3 = slot3;
                                skillSearchResult.Slot2 = slot2;
                                skillSearchResult.Slot1 = slot1;
                                skillSearchResult.AddSkills = new List<CondSkill>(addSkills);
                                skillSearchResult.AmariSkills = new List<CondSkill>(canUseSkills.Where(r=>r.SkillLevel>0));
                                skillSearchResult.UsedGoseki = usedGoseki;
                                skillSearchResult.Fire = head.Fire + body.Fire + arm.Fire + weist.Fire + foot.Fire;
                                skillSearchResult.Water = head.Water + body.Water + arm.Water + weist.Water + foot.Water;
                                skillSearchResult.Thunder = head.Thunder + body.Thunder + arm.Thunder + weist.Thunder + foot.Thunder;
                                skillSearchResult.Ice = head.Ice + body.Ice + arm.Ice + weist.Ice + foot.Ice;
                                skillSearchResult.Dragon = head.Dragon + body.Dragon + arm.Dragon + weist.Dragon + foot.Dragon;
                                

                                skillSearchResults.Add(skillSearchResult);
                                resultCount++;

                                if (resultCount > 10000)
                                {
                                    isAllBreak = true;
                                    break;
                                }
                            }
                            if (isAllBreak) break;
                        }
                        if (isAllBreak) break;
                    }
                    if (isAllBreak) break;
                }
                if (isAllBreak) break;
            }

            InvokeAsync(StateHasChanged);
        }


        private string fileContent = string.Empty;

        private async Task ReadSearchCond(InputFileChangeEventArgs e)
        {
            IBrowserFile file = e.File;
            using var reader = new StreamReader(file.OpenReadStream(maxAllowedSize: 1024 * 1024));
            fileContent = await reader.ReadToEndAsync();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            SkillSearchCond? searchCond = JsonSerializer.Deserialize<SkillSearchCond>(fileContent, options);
            if (searchCond == null)
            {
                return;
            }

            checkedSeriesSkillCount1 = !searchCond.CheckedSeriesSkillCount2;
            checkedSeriesSkillCount2 = searchCond.CheckedSeriesSkillCount2;
            if (searchCond.CheckedSeriesSkillCount2)
            {
                selectedSeriesSkillCount = "2";
            }
            else
            {
                selectedSeriesSkillCount = "1";
            }
            selectedSeriesSkill = searchCond.SelectedSeriesSkill;
            selectedSeriesSkill2 = searchCond.SelectedSeriesSkill2;
            selectedGroupSkill = searchCond.SelectedGroupSkill;
            condSkills = searchCond.CondSkills;
            requiredFire = searchCond.RequiredFire;
            requiredWater = searchCond.RequiredWater;
            requiredThunder = searchCond.RequiredThunder;
            requiredIce = searchCond.RequiredIce;
            requiredDragon = searchCond.RequiredDragon;

            await InvokeAsync(StateHasChanged);
        }

        private async Task SaveSearchCond()
        {
            SkillSearchCond searchCond = new SkillSearchCond();

            searchCond.CheckedSeriesSkillCount2 = checkedSeriesSkillCount2;
            searchCond.SelectedSeriesSkill = selectedSeriesSkill;
            searchCond.SelectedSeriesSkill2 = selectedSeriesSkill2;
            searchCond.SelectedGroupSkill = selectedGroupSkill;
            searchCond.CondSkills = condSkills;
            searchCond.RequiredFire = requiredFire;
            searchCond.RequiredWater = requiredWater;
            searchCond.RequiredThunder = requiredThunder;
            searchCond.RequiredIce = requiredIce;
            searchCond.RequiredDragon = requiredDragon;


            Download d = new Download();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            await d.Execute(JsonSerializer.Serialize(searchCond, options), "検索条件.csv", JS);

            

        }

        /// <summary>
        /// CSVダウンロード
        /// </summary>
        /// <returns></returns>
        private async Task DownloadCsv()
        {
            if (skillSearchResults == null || skillSearchResults.Count == 0)
            {
                return;
            }
            else
            {
                Download d = new Download();
                await d.Execute<SkillSearchResult>(skillSearchResults.ToArray(), searchCondStr, JS);
            }
        }

        ///// <summary>
        ///// CSVダウンロード
        ///// </summary>
        ///// <returns></returns>
        //private async Task DownloadCsv()
        //{
        //    Download d = new Download();
        //    if (armors == null || armors.Count == 0)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        await d.Execute<Armor>(armors.ToArray(), JS);
        //    }

        //    //var csv = new StringBuilder();
        //    //csv.AppendLine(Armor.GetHeader());

        //    //if (armors != null)
        //    //{
        //    //    foreach (var armor in armors)
        //    //    {
        //    //        csv.AppendLine(armor.GetRow());
        //    //    }
        //    //}

        //    //var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        //    //var base64 = Convert.ToBase64String(bytes);
        //    //var dataUrl = $"data:text/csv;base64,{base64}";

        //    //await JS.InvokeVoidAsync("triggerFileDownload", "armor.tsv", dataUrl);
        //}
    }
}
