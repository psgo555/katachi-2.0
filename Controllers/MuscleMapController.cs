using katachi.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace katachi.Controllers
{
    public class MuscleMapController : Controller
    {
        private readonly KatachiDbContext _db;
        private readonly IWebHostEnvironment _env;

        public MuscleMapController(KatachiDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Exercise()
        {
            return View();
        }

        public IActionResult Detail(string muscle, string equipment, string name)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExerciseData(string? key, string? name)
        {
            if (string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("缺少動作參數");
            }

            var query = _db.Exercises
                .AsNoTracking()
                .AsSplitQuery()
                .Include(exercise => exercise.ExerciseEquipment)
                    .ThenInclude(exerciseEquipment => exerciseEquipment.Equipment)
                .Include(exercise => exercise.Goals)
                .Include(exercise => exercise.ExerciseGroupPcts)
                .AsQueryable();

            var exercise = !string.IsNullOrWhiteSpace(key)
                ? await query.FirstOrDefaultAsync(exercise => exercise.ExKey == key)
                : await query.FirstOrDefaultAsync(exercise => exercise.NameZh == name);

            if (exercise == null)
            {
                return NotFound("找不到動作資料");
            }

            var firstGoal = exercise.Goals
                .OrderBy(goal => goal.Id)
                .FirstOrDefault();

            var equipmentName = exercise.ExerciseEquipment?.Equipment?.Name ?? "";
            var muscles = exercise.ExerciseGroupPcts
                .OrderByDescending(groupPct => groupPct.Pct)
                .Select(groupPct => new
                {
                    key = groupPct.GroupKey,
                    name = GetMuscleLabel(groupPct.GroupKey),
                    pct = groupPct.Pct
                })
                .ToList();

            return Json(new
            {
                key = exercise.ExKey,
                name = exercise.NameZh,
                muscle = muscles.FirstOrDefault()?.key ?? "",
                muscleLabel = muscles.FirstOrDefault()?.name ?? "",
                equipment = ToEquipmentKey(equipmentName),
                equipmentLabel = equipmentName,
                difficulty = GetDifficulty(firstGoal?.Sets ?? 0),
                muscles,
                desc = BuildDescription(exercise.NameZh, equipmentName, firstGoal?.Sets, firstGoal?.Reps, firstGoal?.RestSeconds, muscles.Select(muscle => muscle.name)),
                sets = firstGoal?.Sets,
                reps = firstGoal?.Reps,
                restSeconds = firstGoal?.RestSeconds,
                img = GetExerciseImageUrl(exercise.ExKey, exercise.NameZh)
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExerciseList()
        {
            var exercises = await _db.Exercises
                .AsNoTracking()
                .AsSplitQuery()
                .Include(exercise => exercise.ExerciseEquipment)
                    .ThenInclude(exerciseEquipment => exerciseEquipment.Equipment)
                .Include(exercise => exercise.Goals)
                .Include(exercise => exercise.ExerciseGroupPcts)
                .ToListAsync();

            var data = exercises
                .Select(exercise =>
                {
                    var firstGoal = exercise.Goals
                        .OrderBy(goal => goal.Id)
                        .FirstOrDefault();

                    var equipmentName = exercise.ExerciseEquipment?.Equipment?.Name ?? "";
                    var muscles = exercise.ExerciseGroupPcts
                        .OrderByDescending(groupPct => groupPct.Pct)
                        .Select(groupPct => new
                        {
                            key = groupPct.GroupKey,
                            name = GetMuscleLabel(groupPct.GroupKey),
                            pct = groupPct.Pct
                        })
                        .ToList();

                    return new
                    {
                        key = exercise.ExKey,
                        name = exercise.NameZh,
                        muscle = muscles.FirstOrDefault()?.key ?? "",
                        muscleLabel = muscles.FirstOrDefault()?.name ?? "",
                        equipment = ToEquipmentKey(equipmentName),
                        equipmentLabel = equipmentName,
                        difficulty = GetDifficulty(firstGoal?.Sets ?? 0),
                        muscles,
                        desc = BuildDescription(exercise.NameZh, equipmentName, firstGoal?.Sets, firstGoal?.Reps, firstGoal?.RestSeconds, muscles.Select(muscle => muscle.name)),
                        sets = firstGoal?.Sets,
                        reps = firstGoal?.Reps,
                        restSeconds = firstGoal?.RestSeconds,
                        img = GetExerciseImageUrl(exercise.ExKey, exercise.NameZh)
                    };
                })
                .ToList();

            return Json(data);
        }
        private static string ToEquipmentKey(string equipmentName)
        {
            return equipmentName switch
            {
                "啞鈴" => "dumbbell",
                "槓鈴" => "barbell",
                "徒手" or "徒手訓練" or "自體重量" => "bodyweight",
                "機械式" or "機械" or "器械" => "machine",
                _ => equipmentName
            };
        }

        private static string GetMuscleLabel(string groupKey)
        {
            return groupKey switch
            {
                "chest" or "胸" or "胸部" => "胸",
                "back" or "背" or "背部" => "背",
                "shoulder" or "shoulders" or "肩" or "肩部" => "肩",
                "arms" or "arm" or "手臂" => "手臂",
                "core" or "核心" => "核心",
                "legs" or "leg" or "腿" or "腿部" => "腿",
                "glute" or "臀" or "臀部" => "臀",
                _ => groupKey
            };
        }

        private static int GetDifficulty(int sets)
        {
            return sets switch
            {
                >= 5 => 4,
                >= 4 => 3,
                >= 3 => 2,
                _ => 1
            };
        }

        private static string BuildDescription(string name, string equipmentName, int? sets, string? reps, string? restSeconds, IEnumerable<string> muscles)
        {
            var muscleText = string.Join("、", muscles.Where(muscle => !string.IsNullOrWhiteSpace(muscle)));
            var equipmentText = string.IsNullOrWhiteSpace(equipmentName) ? "適合器材" : equipmentName;
            var prescription = sets.HasValue && !string.IsNullOrWhiteSpace(reps)
                ? $"建議執行 <strong>{sets} 組 × {reps}</strong>"
                : "請依照訓練目標安排組數與次數";
            var rest = string.IsNullOrWhiteSpace(restSeconds)
                ? ""
                : $"，組間休息 <strong>{restSeconds}</strong>";

            return $"{name} 是使用 <strong>{equipmentText}</strong> 進行的訓練動作，主要刺激 <strong>{muscleText}</strong>。{prescription}{rest}。進行時請保持動作穩定、核心收緊，避免用代償方式完成動作。";
        }

        private string GetExerciseImageUrl(string exKey, string name)
        {
            var extensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            var folders = new[]
            {
                new { Physical = Path.Combine(_env.WebRootPath, "images", "exercises"), Url = "/images/exercises" },
                new { Physical = Path.Combine(_env.WebRootPath, "images"), Url = "/images" }
            };

            foreach (var folder in folders)
            {
                foreach (var baseName in GetExerciseImageBaseNames(exKey, name))
                {
                    foreach (var extension in extensions)
                    {
                        var fileName = $"{baseName}{extension}";
                        var physicalPath = Path.Combine(folder.Physical, fileName);

                        if (System.IO.File.Exists(physicalPath))
                        {
                            return $"{folder.Url}/{Uri.EscapeDataString(fileName)}";
                        }
                    }
                }
            }

            return "";
        }

        private static IEnumerable<string> GetExerciseImageBaseNames(string exKey, string name)
        {
            var candidates = new List<string?> { exKey, name };

            switch (exKey)
            {
                case "pushup":
                    candidates.Add("push-up");
                    break;
                case "bench-db":
                    candidates.Add("dumbbell_bench_press");
                    break;
            }

            switch (name)
            {
                case "伏地挺身":
                    candidates.Add("push-up");
                    break;
                case "啞鈴臥推":
                    candidates.Add("dumbbell_bench_press");
                    break;
            }

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Select(candidate => candidate!)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
