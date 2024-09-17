

# Fire Rescuing Agent using Q-Learning and Reinforcement Learning

This project implements an autonomous agent using Q-learning and Reinforcement Learning (RL) to navigate through a fire-affected environment and rescue hostages. The environment is simulated as a grid, where the agent learns to find the optimal path to reach and rescue hostages while avoiding fire and obstacles.

## Table of Contents
- [Introduction](#introduction)
- [Problem Statement](#problem-statement)
- [Methodology](#methodology)
  - [1. Simulation Environment Setup](#1-simulation-environment-setup)
  - [2. Agent Actions](#2-agent-actions)
  - [3. Dynamic Fire Simulation](#3-dynamic-fire-simulation)
  - [4. Integration of AI with Unity](#4-integration-of-ai-with-unity)
- [Implementation](#implementation)
  - [Algorithm Initialization](#algorithm-initialization)
  - [Reward System](#reward-system)
  - [Q-value Updates](#q-value-updates)
- [Results](#results)
- [Conclusion](#conclusion)

## Introduction
Fire incidents pose significant risks to human life, and timely rescue is critical. This project introduces an AI-based autonomous agent that uses Q-learning to navigate fire-affected environments, rescue hostages, and find the safest escape routes. This solution can reduce human risks by deploying AI in dangerous rescue operations.

## Problem Statement
Rescuing individuals trapped in fire-affected zones is a critical challenge due to the complexity and danger involved in navigating such environments. Traditional rescue operations expose humans to significant risk. The goal of this project is to create an autonomous agent that can efficiently find and rescue hostages from fire zones in an optimal and safe manner.

## Methodology

### 1. Simulation Environment Setup
The simulation environment is grid-based, representing the structure of a building. Each cell in the grid can represent:
- Empty space
- Hostage location
- Fire
- Obstacles (walls, debris)

The environment mimics real-world challenges, offering complex scenarios for the agent to navigate.

### 2. Agent Actions
The agent can move in four directions: up, down, left, and right. These actions are decided using a Q-learning algorithm based on the current state of the environment, balancing safety and rescue speed.

### 3. Dynamic Fire Simulation
The spread of fire is simulated dynamically using the Unity engine. Fire spreads randomly across the grid, creating an unpredictable environment that forces the agent to make quick, intelligent decisions. Factors such as room layout and obstacles influence the fire's spread.

### 4. Integration of AI with Unity
The Q-learning algorithm is integrated with Unity for real-time feedback and visualization. The agent's path and strategy are adjusted dynamically based on the changing environment, providing a realistic simulation of fire rescue operations.

## Implementation

### Algorithm Initialization
The Q-learning algorithm initializes a Q-table where each state (cell in the grid) has an associated action (move up, down, left, or right). Initially, all Q-values are set to zero, indicating no prior knowledge of the environment.

### Reward System
- **Positive Rewards:** Moving closer to the hostage earns positive rewards, encouraging the agent to find the quickest route.
- **Negative Rewards:** Moving toward fire or obstacles results in negative rewards to prevent dangerous or inefficient movements.
- **Threshold for Negative Rewards:** If negative rewards reach a certain threshold, the mission is considered a failure to ensure the agent avoids high-risk areas.

### Q-value Updates
The Q-value updates as the agent learns from its experiences:

```
Q(s, a) = Q(s, a) + α [ R(s, a) + γ max_{a'} Q(s', a') - Q(s, a) ]
```

Where:
- `Q(s, a)` is the current Q-value for action `a` in state `s`.
- `α` is the learning rate.
- `R(s, a)` is the reward for taking action `a` in state `s`.
- `γ` is the discount factor for future rewards.
- `max_{a'} Q(s', a')` is the maximum Q-value for the next state `s'`.

## Results
The results show that the Q-learning agent significantly improves navigation efficiency and safety. The agent learns to avoid fire while optimizing the rescue path, reducing rescue time and minimizing risk.

## Conclusion
This project demonstrates the potential of AI and reinforcement learning in enhancing fire rescue operations. The autonomous agent's ability to adapt to dynamic environments and optimize rescue strategies has promising implications for real-world emergency response.

