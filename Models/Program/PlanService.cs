using Microsoft.EntityFrameworkCore;
using katachi.Models.Shop;

namespace katachi.Models.Program
{
    public class PlanService
    {
        private readonly KatachiDbContext _db;

        public PlanService(KatachiDbContext db)
        {
            _db = db;
        }

        public PlanResult Generate(GenerateRequest req)
        {
            var result = new PlanResult
            {
                WeekDays = new List<DayPlan>(),
                Prescription = GetPrescription(req.Goal)
            };

            var selectedEquipment = NormalizeEquipment(req.Equipment);

            var templates = _db.DayTemplates
                .AsNoTracking()
                .Where(template => template.TotalDays == req.Days)
                .OrderBy(template => template.DayNumber)
                .ToList();

            foreach (var template in templates)
            {
                var dayTemplateExercises = _db.DayTemplateExercises
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(dayTemplateExercise => dayTemplateExercise.Exercise)
                        .ThenInclude(exercise => exercise.ExerciseEquipment)
                            .ThenInclude(exerciseEquipment => exerciseEquipment.Equipment)
                    .Include(dayTemplateExercise => dayTemplateExercise.Exercise)
                        .ThenInclude(exercise => exercise.Goals)
                    .Include(dayTemplateExercise => dayTemplateExercise.Exercise)
                        .ThenInclude(exercise => exercise.ExerciseGroupPcts)
                    .Where(dayTemplateExercise =>
                        dayTemplateExercise.DayTemplateId == template.Id &&
                        dayTemplateExercise.Exercise.ExerciseEquipment != null &&
                        dayTemplateExercise.Exercise.Goals.Any(goal => goal.Goal == req.Goal))
                    .OrderBy(dayTemplateExercise => dayTemplateExercise.SortOrder)
                    .ToList();

                var exercises = dayTemplateExercises
                    .Where(dayTemplateExercise =>
                    {
                        var equipmentName = dayTemplateExercise.Exercise.ExerciseEquipment?.Equipment?.Name;
                        return !string.IsNullOrWhiteSpace(equipmentName) && selectedEquipment.Contains(equipmentName);
                    })
                    .Select(dayTemplateExercise =>
                    {
                        var exercise = dayTemplateExercise.Exercise;
                        var goal = exercise.Goals.First(goal => goal.Goal == req.Goal);

                        return new ExerciseItem
                        {
                            ExKey = exercise.ExKey,
                            Name = exercise.NameZh,
                            MuscleGroup = string.Join(",", exercise.ExerciseGroupPcts
                                .OrderByDescending(groupPct => groupPct.Pct)
                                .Select(groupPct => groupPct.GroupKey)),
                            Equipment = exercise.ExerciseEquipment!.Equipment.Name,
                            Sets = goal.Sets,
                            Reps = goal.Reps,
                            RestSeconds = goal.RestSeconds
                        };
                    })
                    .ToList();

                result.WeekDays.Add(new DayPlan
                {
                    DayNumber = template.DayNumber,
                    DayName = template.DayName,
                    Exercises = exercises
                });
            }

            return result;
        }

        private static HashSet<string> NormalizeEquipment(List<string> equipment)
        {
            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in equipment.Where(item => !string.IsNullOrWhiteSpace(item)))
            {
                normalized.Add(item);

                if (item == "機械式")
                {
                    normalized.Add("機械");
                    normalized.Add("器械");
                }

                if (item == "徒手")
                {
                    normalized.Add("徒手訓練");
                    normalized.Add("自體重量");
                }
            }

            return normalized;
        }

        private Prescription GetPrescription(string goal)
        {
            return goal switch
            {
                "hypertrophy" => new Prescription { Sets = 4, Reps = "8–12", Rest = "60–90 秒" },
                "strength" => new Prescription { Sets = 5, Reps = "3–5", Rest = "3–5 分鐘" },
                "fatloss" => new Prescription { Sets = 3, Reps = "15–20", Rest = "30–45 秒" },
                _ => new Prescription { Sets = 4, Reps = "8–12", Rest = "60–90 秒" }
            };
        }
    }
}
