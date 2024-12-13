import glob
import os
import pandas as pd

path = os.getcwd()
csv_all = glob.glob(os.path.join(path, "*.csv"))
data = pd.concat((pd.read_csv(file) for file in csv_all), ignore_index=True)

players = data.groupby("player")

for player, group_data in players:
    new_csv_player = f"{player}.csv"
    group_data.to_csv(new_csv_player, index=False)
    print(f"data for {player} saved to {new_csv_player}")

teams = data.groupby("team")

for team, group_data in teams:
    new_csv_team = f"{team}.csv"
    group_data.to_csv(new_csv_team, index=False)
    print(f"data for {team} saved to{new_csv_team}")

