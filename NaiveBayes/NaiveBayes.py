# Example of Naive Bayes implemented from Scratch in Python
import csv
import random
import math

initProbs = {}  # 初始概率，即根据训练集得到的发病/不发病概率


def loadCsv(filename):
    lines = csv.reader(open(filename, "r"))
    dataset = list(lines)
    for i in range(len(dataset)):
        dataset[i] = [float(x) for x in dataset[i]]
    return dataset


def splitDataset(dataset, splitRatio):
    trainSize = int(len(dataset) * splitRatio)
    trainSet = []
    copy = list(dataset)
    while len(trainSet) < trainSize:
        index = random.randrange(len(copy))
        trainSet.append(copy.pop(index))
    return [trainSet, copy]


def separateByClass(dataset):
    separated = {}
    for i in range(len(dataset)):
        vector = dataset[i]
        if (vector[-1] not in separated):
            separated[vector[-1]] = []
        separated[vector[-1]].append(vector)
    for classValue, items in separated.items():
        initProbs[classValue] = len(items) / float(len(dataset))
    return separated


def mean(numbers):
    return sum(numbers) / float(len(numbers))


def stdev(numbers):
    avg = mean(numbers)
    variance = sum([pow(x - avg, 2) for x in numbers]) / float(len(numbers) - 1)
    return math.sqrt(variance)


def summarize(dataset):
    summaries = [(mean(attribute), stdev(attribute)) for attribute in zip(*dataset)]  # 注意zip函数和*的用法
    del summaries[-1]
    return summaries


def summarizeByClass(dataset):
    separated = separateByClass(dataset)
    summaries = {}
    for classValue, instances in separated.items():
        summaries[classValue] = summarize(instances)
    return summaries


def calculateProbability(x, mean, stdev):
    exponent = math.exp(-(math.pow(x - mean, 2) / (2 * math.pow(stdev, 2))))
    return (1 / (math.sqrt(2 * math.pi) * stdev)) * exponent


def calculateClassProbabilities(summaries, inputVector, withInitProb):
    probabilities = {}
    for classValue, classSummaries in summaries.items():
        if withInitProb:
            probabilities[classValue] = initProbs[classValue]
        else:
            probabilities[classValue] = 1
        for i in range(len(classSummaries)):
            mean, stdev = classSummaries[i]
            x = inputVector[i]
            probabilities[classValue] *= calculateProbability(x, mean, stdev)
    return probabilities


def predict(summaries, inputVector, withInitProb):
    probabilities = calculateClassProbabilities(summaries, inputVector, withInitProb)
    bestLabel, bestProb = None, -1
    for classValue, probability in probabilities.items():
        if bestLabel is None or probability > bestProb:
            bestProb = probability
            bestLabel = classValue
    return bestLabel


def getPredictions(summaries, testSet, withInitProbs):
    predictions = []
    for i in range(len(testSet)):
        result = predict(summaries, testSet[i], withInitProbs)
        predictions.append(result)
    return predictions


def getAccuracy(testSet, predictions):
    correct = 0
    for i in range(len(testSet)):
        if testSet[i][-1] == predictions[i]:
            correct += 1
    return (correct / float(len(testSet))) * 100.0


def main():
    filename = 'pima-indians-diabetes.data.csv'
    splitRatio = 0.67
    dataset = loadCsv(filename)
    count = 0
    for i in range(1000):
        trainingSet, testSet = splitDataset(dataset, splitRatio)
        # print('Split {0} rows into train={1} and test={2} rows'.format(len(dataset), len(trainingSet), len(testSet)))
        # prepare model
        summaries = summarizeByClass(trainingSet)
        # print('initProb of 0 is {0}, and initProb of 1 is {1}'.format(initProbs[0],initProbs[1]))
        # test model with initProb
        predictions = getPredictions(summaries, testSet, True)
        accuracyWithInitProb = getAccuracy(testSet, predictions)
        print('Accuracy with initProb: {0}%'.format(accuracyWithInitProb))
        # test model without initProb
        predictions = getPredictions(summaries, testSet, False)
        accuracyWithoutInitProb = getAccuracy(testSet, predictions)
        print('Accuracy without initProb: {0}%'.format(accuracyWithoutInitProb))
        if accuracyWithInitProb < accuracyWithoutInitProb:
            count += 1
    print(count)


main()
print("over")
