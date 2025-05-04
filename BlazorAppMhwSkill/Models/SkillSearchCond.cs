namespace BlazorAppMhwSkill.Models
{
    public class SkillSearchCond
    {
        public bool CheckedSeriesSkillCount2 { get; set; }
        public string? SelectedSeriesSkill {  get; set; }
        public string? SelectedSeriesSkill2 { get; set; }
        public string? SelectedGroupSkill { get; set; }
        public List<CondSkill> CondSkills { get; set; } = new List<CondSkill>();
        public int RequiredFire {  get; set; }
        public int RequiredWater { get; set; }
        public int RequiredThunder { get; set; }
        public int RequiredIce { get; set; }
        public int RequiredDragon { get; set; }

    }
}
