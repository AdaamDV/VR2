import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.stats import gaussian_kde
import plotly.express as px

# path = os.getcwd()
# csv_all = glob.glob(os.path.join(path, "*.csv"))

# data = pd.concat((pd.read_csv(file) for file in csv_all), ignore_index=True)

# user input parameters from unity
input_user = input("For which player/team would you like to see the heatmap?")

player_team_csv = f"{input_user}.csv"
chosen_csv = pd.read_csv(player_team_csv)
# player_data = data[data["player"].isin([input_user])]

x = chosen_csv["shotX"]
y = chosen_csv["shotY"]
nbins = 1000
k = gaussian_kde([x,y])
xi, yi = np.mgrid[x.min():x.max():nbins*1j,y.min():y.max():nbins*1j]
zi = k(np.vstack([xi.flatten(),yi.flatten()])).reshape(xi.shape)
fig, ax = plt.subplots(figsize=(8,8))
color = ax.pcolormesh(xi, yi, zi)
ax.set_xlim([0, 50])
ax.set_ylim([0, 94])
plt.show()

output_file = "heatmap.png"
plt.savefig(output_file, dpi=500, bbox_inches="tight")
plt.close

plot = px.scatter(chosen_csv, x="shotX", y="shotY", hover_data={"time_remaining": True})
plot.update_layout(xaxis=dict(range=[0,50]), yaxis=dict(range=[0,94]))

plot.show()
plt.show()


plt.subplots()