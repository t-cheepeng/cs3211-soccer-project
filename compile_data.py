import json
import os
from json import JSONDecodeError

num_shot = 0
num_dribble = 0
num_pass = 0
num_pass_failure = 0
num_pass_failure_REAL = 0
num_pass_failure_FAKE = 0

# Clone git repo https://github.com/statsbomb/open-data/trunk/data/events
# Beware it's large. About 1.2GB in size.
# Put this python file into /data/events folder with the .json data soccer statistics
# See documentation under "Open Data Events v4.0.0" for description of the event type
# Just performs a simple counting of number of Shoot, Dribble(called Carry in the data) and Pass
# Dribble doesn't exist as an event but we just assume that it is the same as Dribble
# Outputs the proportion(percentage) of the events to console
LIMIT = 10
count = 0
has_outcome = 0
files = [f for f in os.listdir('.') if os.path.isfile(f)]
print("Total files: ", len(files))
print("Scan files: ", LIMIT)
for f in files:
    count += 1
    try:
        with open(f, encoding="utf8") as json_file:
            data = json.load(json_file)
        for obj in data:
            event_name = obj["type"]["name"]
            if event_name == "Shot":
             num_shot += 1

            # Carry is closer to dribble than "Dribble" is according to data format explanation in statsbomb
            if event_name == "Carry":
                num_dribble += 1

            if event_name == "Pass":
                num_pass += 1

                if "outcome" in obj["pass"]:
                    has_outcome += 1

            if event_name == "Interception":
                num_pass_failure += 1
                if "outcome" in obj["interception"] and 15 <= obj["interception"]["outcome"]["id"] <= 17:
                    num_pass_failure_REAL += 1
                else:
                    num_pass_failure_FAKE += 1

    except JSONDecodeError:
        print("Too lazy to exclude the python file. So just catch it here", f)
    if count >= LIMIT:
        break
        
allEvent = num_shot + 2 * num_dribble + num_pass
shot_prob = num_shot / allEvent
dribble_prob = num_dribble / allEvent
pass_prob = num_pass / allEvent

print("Shoot Prob: ", shot_prob)
print("Dribble/Run Prob: ", dribble_prob)
print("Passing Prob: ", pass_prob)

print("Passing with outcome: ", has_outcome)
print("Passing no outcome: ", num_pass-has_outcome)

print("Intercepted (total): ", num_pass_failure)
print("Intercepted (success): ", num_pass_failure_REAL)
print("Intercepted (failed): ", num_pass_failure_FAKE)